using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace XamlBrewer.Universal.Controls
{
    public class PullToRefreshListView : ListView
    {
        private const string ScrollViewerControl = "ScrollViewer";
        private const string ContainerGrid = "ContainerGrid";
        private const string PullToRefreshIndicator = "PullToRefreshIndicator";
        private const string RefreshButton = "RefreshButton";
        private const string VisualStateNormal = "Normal";
        private const string VisualStateReadyToRefresh = "ReadyToRefresh";

        private DispatcherTimer compressionTimer;
        private ScrollViewer scrollViewer;
        private DispatcherTimer timer;
        private Grid containerGrid;
        private Border pullToRefreshIndicator;
        private bool isCompressionTimerRunning;
        private bool isReadyToRefresh;
        private bool isCompressedEnough;

        public event EventHandler RefreshContent;

        public static readonly DependencyProperty PullTextProperty = DependencyProperty.Register("PullText", typeof(string), typeof(PullToRefreshListView), new PropertyMetadata("Pull to refresh"));
        public static readonly DependencyProperty RefreshTextProperty = DependencyProperty.Register("RefreshText", typeof(string), typeof(PullToRefreshListView), new PropertyMetadata("Release to refresh"));
        public static readonly DependencyProperty RefreshHeaderHeightProperty = DependencyProperty.Register("RefreshHeaderHeight", typeof(double), typeof(PullToRefreshListView), new PropertyMetadata(40D));
        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register("RefreshCommand", typeof(ICommand), typeof(PullToRefreshListView), new PropertyMetadata(null));
        public static readonly DependencyProperty ArrowColorProperty = DependencyProperty.Register("ArrowColor", typeof(Brush), typeof(PullToRefreshListView), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

#if WINDOWS_PHONE_APP
        private double offsetTreshhold = 40;
#endif
#if WINDOWS_APP
        private double offsetTreshhold = 40;
#endif

        public PullToRefreshListView()
        {
            this.DefaultStyleKey = typeof(PullToRefreshListView);
            Loaded += PullToRefreshScrollViewer_Loaded;
        }

        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }

        public double RefreshHeaderHeight
        {
            get { return (double)GetValue(RefreshHeaderHeightProperty); }
            set { SetValue(RefreshHeaderHeightProperty, value); }
        }

        public string RefreshText
        {
            get { return (string)GetValue(RefreshTextProperty); }
            set { SetValue(RefreshTextProperty, value); }
        }

        public string PullText
        {
            get { return (string)GetValue(PullTextProperty); }
            set { SetValue(PullTextProperty, value); }
        }

        public Brush ArrowColor
        {
            get { return (Brush)GetValue(ArrowColorProperty); }
            set { SetValue(ArrowColorProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            scrollViewer = (ScrollViewer)GetTemplateChild(ScrollViewerControl);
            scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            scrollViewer.Margin = new Thickness(0, 0, 0, -RefreshHeaderHeight);
            var transform = new CompositeTransform();
            transform.TranslateY = -RefreshHeaderHeight;
            scrollViewer.RenderTransform = transform;

            containerGrid = (Grid)GetTemplateChild(ContainerGrid);
            pullToRefreshIndicator = (Border)GetTemplateChild(PullToRefreshIndicator);
            SizeChanged += OnSizeChanged;
        }

        /// <summary>
        /// Initiate timers to detect if we're scrolling into negative space
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PullToRefreshScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            // Show Refresh Button on non-touch device.
            if (new Windows.Devices.Input.TouchCapabilities().TouchPresent == 0)
            {
                var refreshButton = (Button)GetTemplateChild(RefreshButton);
                refreshButton.Visibility = Visibility.Visible;
                refreshButton.Click += RefreshButton_Click;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;

            compressionTimer = new DispatcherTimer();
            compressionTimer.Interval = TimeSpan.FromSeconds(.5);
            compressionTimer.Tick += CompressionTimer_Tick;

            timer.Start();
        }

        /// <summary>
        /// Clip the bounds of the control to avoid showing the pull to refresh text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
        }

        /// <summary>
        /// Detect if we've scrolled all the way to the top. Stop timers when we're not completely in the top
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (e.NextView.VerticalOffset == 0)
            {
                timer.Start();
            }
            else
            {
                if (timer != null)
                {
                    timer.Stop();
                }

                if (compressionTimer != null)
                {
                    compressionTimer.Stop();
                }

                isCompressionTimerRunning = false;
                isCompressedEnough = false;
                isReadyToRefresh = false;

                VisualStateManager.GoToState(this, VisualStateNormal, true);
            }
        }

        /// <summary>
        /// Detect if I've scrolled far enough and been there for enough time to refresh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompressionTimer_Tick(object sender, object e)
        {
            if (isCompressedEnough)
            {
                VisualStateManager.GoToState(this, VisualStateReadyToRefresh, true);
                isReadyToRefresh = true;
            }
            else
            {
                isCompressedEnough = false;
                compressionTimer.Stop();
            }
        }

        /// <summary>
        /// Invoke timer if we've scrolled far enough up into negative space. If we get back to offset 0 the refresh command and event is invoked. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, object e)
        {
            if (containerGrid != null)
            {
                Rect elementBounds = pullToRefreshIndicator.TransformToVisual(containerGrid).TransformBounds(new Rect(0.0, 0.0, pullToRefreshIndicator.Height, RefreshHeaderHeight));
                var compressionOffset = elementBounds.Bottom;

                if (compressionOffset > offsetTreshhold)
                {
                    if (isCompressionTimerRunning == false)
                    {
                        isCompressionTimerRunning = true;
                        compressionTimer.Start();
                    }

                    isCompressedEnough = true;
                }
                else if (compressionOffset == 0 && isReadyToRefresh == true)
                {
                    InvokeRefresh();
                }
                else
                {
                    isCompressedEnough = false;
                    isCompressionTimerRunning = false;
                }
            }
        }

        private void RefreshButton_Click(object sender, object e)
        {
            InvokeRefresh();
        }

        /// <summary>
        /// Set correct visual state and invoke refresh event and command
        /// </summary>
        private void InvokeRefresh()
        {
            isReadyToRefresh = false;
            VisualStateManager.GoToState(this, VisualStateNormal, true);

            if (RefreshContent != null)
            {
                RefreshContent(this, EventArgs.Empty);
            }

            if (RefreshCommand != null && RefreshCommand.CanExecute(null) == true)
            {
                RefreshCommand.Execute(null);
            }
        }
    }
}

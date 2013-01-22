using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace GamesSample
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GetUriPage : GamesSample.Common.LayoutAwarePage
    {
        // Channel created by the app to receive notifications
        static PushNotificationChannel pushNotificationChannel;
        
        public GetUriPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        #region "Notifications"

        /// <summary>
        /// Create the notification channel
        /// </summary>
        void OpenChannel()
        {
            var operation = PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            operation.Completed += OnChannelCreationCompleted;
            //operation.Start();
            operation.GetResults();
        }

        /// <summary>
        /// Completed Channel creation handler
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="status"></param>
        void OnChannelCreationCompleted(IAsyncOperation<PushNotificationChannel> operation, AsyncStatus status)
        {
            if (operation.Status == AsyncStatus.Completed)
            {
                pushNotificationChannel = operation.GetResults();

                // Send these values together with user data to your server of choice
                var notificationUri = pushNotificationChannel.Uri;
                var expirationTime = pushNotificationChannel.ExpirationTime;
            }
        }

        /// <summary>
        /// Receive notification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnPushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs e)
        {
            string notificationContent = String.Empty;
            switch (e.NotificationType)
            {
                case PushNotificationType.Badge:
                    notificationContent = e.BadgeNotification.Content.GetXml();
                    break;
                case PushNotificationType.Tile:
                    notificationContent = e.TileNotification.Content.GetXml();
                    break;
                case PushNotificationType.Toast:
                    notificationContent = e.ToastNotification.Content.GetXml();
                    break;
            }

            // prevent the notification from being delivered to the UI
            e.Cancel = true;
        }

        #endregion

        #region "Buttons"

        private void btGetUri_Click(object sender, RoutedEventArgs e)
        {
            OpenChannel();
            pushNotificationChannel.PushNotificationReceived += OnPushNotificationReceived;
            tbUri.Text = pushNotificationChannel.Uri;
        }

        /// <summary>
        /// Method for sending channel uri to the cloud service and sending push notification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSendUriByPost_Click(object sender, RoutedEventArgs e)
        {
            PostUri();
        }

        #endregion

        #region "WebApi access"

        /// <summary>
        /// Send uri to the cloud service
        /// </summary>
        public void PostUri()
        {
            // Create a new product
            Uri uri = new Uri(tbUri.Text);

            // Client for the cloud service webapi
            var client = new HttpClient();

            // Cloud service adress
            client.BaseAddress = new Uri("http://localhost:55505/");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.PostAsync("api/values/", null).Result;
            if (response.IsSuccessStatusCode)
            {
                //gizmoUri = response.Headers.Location;
            }
            else
            {   
                tbResponse.Text = String.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
        }

        #endregion
    }
}

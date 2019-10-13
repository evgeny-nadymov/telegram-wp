using Windows.ApplicationModel.Resources;

namespace Telegram.Api.Resources
{
    internal class AppResources
    {
        private static ResourceLoader resourceMan;

        //private static global::System.Globalization.CultureInfo resourceCulture;

        internal AppResources()
        {
        }

        internal static ResourceLoader ResourceManager
        {
            get
            {
                if (ReferenceEquals(resourceMan, null))
                {
                    var temp = ResourceLoader.GetForViewIndependentUse("TelegramClient.Tasks/Resources");
                    resourceMan = temp;
                }

                return resourceMan;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Deleted User.
        /// </summary>
        internal static string DeletedUser
        {
            get
            {
                return ResourceManager.GetString("DeletedUser");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Empty User.
        /// </summary>
        internal static string EmptyUser
        {
            get
            {
                return ResourceManager.GetString("EmptyUser");
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Saved Messages.
        /// </summary>
        internal static string SavedMessages
        {
            get
            {
                return ResourceManager.GetString("SavedMessages");
            }
        }
    }
}
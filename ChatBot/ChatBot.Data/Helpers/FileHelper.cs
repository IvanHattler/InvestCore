namespace ChatBot.Data.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Path to data store directory
        /// </summary>
        public static readonly string DirectoryPath;

        static FileHelper()
        {
            DirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft",
                "UserSecrets",
                "f50f26cc-7146-4fa7-949c-c9751da6f0f4");
        }
    }
}

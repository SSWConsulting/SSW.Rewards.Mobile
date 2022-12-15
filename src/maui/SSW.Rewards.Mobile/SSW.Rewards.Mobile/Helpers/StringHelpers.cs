namespace SSW.Rewards.Helpers
{
    public static class StringHelpers
    {
        public static string Base64Decode(string data)
        {
            try
            {
                var encodedBytes = Convert.FromBase64String(data);
                return System.Text.Encoding.UTF8.GetString(encodedBytes);
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}

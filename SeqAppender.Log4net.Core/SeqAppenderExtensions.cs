using Microsoft.AspNetCore.Hosting;

namespace SeqAppender.Log4net.Core
{
    public static class SeqAppenderExtensions
    {
        internal static string EnvironmentName = "";
        
        /// <summary>
        /// Log4net DI kullanmadığı için, EnvironmentName'i parametre geçmek için bu metotun çağırılması gerekir.
        /// </summary>
        /// <param name="env"></param>
        public static void ConfigureSeqAppender(this IHostingEnvironment env)
        {
            EnvironmentName = env.EnvironmentName;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncIO
{
    public static class Tasks
    {


        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the synchronous way and can be used to compare the performace of sync \ async approaches. 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContent(this IEnumerable<Uri> uris) 
        {
            return uris.Select(uri => new WebClient().DownloadString(uri));
        }



        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the asynchronous way and can be used to compare the performace of sync \ async approaches. 
        /// 
        ///  
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <param name="maxConcurrentStreams">Max count of concurrent request streams</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContentAsync(this IEnumerable<Uri> uris, int maxConcurrentStreams)
        {
            return Helper(uris, maxConcurrentStreams).GetAwaiter().GetResult();
        }

        public static async Task<IEnumerable<string>> Helper(IEnumerable<Uri> uris, int maxConcurrentStreams)
        {
            var queue = new Queue<Uri>(uris);
            var tasks = new List<Task<string>>(maxConcurrentStreams);

            while (queue.Count > 0 && tasks.Count < maxConcurrentStreams)
            {
                tasks.Add(new WebClient().DownloadStringTaskAsync(queue.Dequeue()));
            }

            while (tasks.Count > 0)
            {
                var taskDone = await Task.WhenAny(tasks);
                tasks.Remove(taskDone);

                if (queue.Count > 0)
                {
                    tasks.Add(new WebClient().DownloadStringTaskAsync(queue.Dequeue()));
                }
            }

            return tasks.Select(task => task.Result);
        }

        /// <summary>
        /// Calculates MD5 hash of required resource.
        /// 
        /// Method has to run asynchronous. 
        /// Resource can be any of type: http page, ftp file or local file.
        /// </summary>
        /// <param name="resource">Uri of resource</param>
        /// <returns>MD5 hash</returns>
        public static async Task<string> GetMD5Async(this Uri resource)
        {
            byte[] hash = MD5.Create()
                .ComputeHash(await new WebClient().DownloadDataTaskAsync(resource));

            return String.Concat(hash.Select(b => b.ToString("x2")));
        }

    }



}

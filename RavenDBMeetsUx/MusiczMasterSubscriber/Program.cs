using System;
using System.Threading;
using Raven.Abstractions.Data;
using Raven.Client.Document;

namespace MusiczMasterSubscriber
{
    class Program
    {
        public static bool end = false;

        static void Main(string[] args)
        {
            using (var store = new DocumentStore { Url = "http://localhost:8080/", DefaultDatabase = "Music" }.Initialize())
            {
                var obs = store.Changes().ForDocument("playlists/1").Subscribe(new Tracker());
                Console.WriteLine("Listening for changes...");

                while (!end)
                {
                    Thread.Sleep(100);
                }
                Console.ReadKey();
            }
        }
    }

    public class Tracker : IObserver<DocumentChangeNotification>
    {
        public void OnNext(DocumentChangeNotification value)
        {
            Console.WriteLine("Change detected on document " + value.Name);

            // TODO stuff
        }

        public void OnError(Exception error)
        {
            Console.WriteLine(error);
            Program.end = true;
        }

        public void OnCompleted()
        {
            Console.WriteLine("EOF");
            Program.end = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using ReactiveUI;
using SQLite;

namespace SearchDemo
{
    public class MainViewModel : ReactiveObject
    {
        private IDisposable subscription = null;

        public MainViewModel()
        {
            Results = new ObservableCollection<Album>();
        }

        public ObservableCollection<Album> Results { get; set; }

        public void WhenTextChanged(IObservable<string> textChangeObservable)
        {
            // clear the existing subscription
            if (subscription != null)
                subscription.Dispose();

            subscription = textChangeObservable.Subscribe(next =>
            {
                Results.Clear();

                SearchFor(next).ToObservable()
                    .ObserveOn(SynchronizationContext.Current)
                    .Finally(() => { Count = string.Format("Found {0} results", Results.Count); })
                    .Subscribe(Results.Add);
            });

        }

        static IEnumerable<Album> SearchFor(string text)
        {
            var dbPath = GetDatabasePath();
            var db = new SQLiteConnection(dbPath);
            return db.Table<Album>()
                     .ToList() // reversed because i'm sick of fighting with sqlite
                     .Where(a => a.Title.ToLowerInvariant().Contains(text.ToLowerInvariant()));
        }

        static string GetDatabasePath()
        {
            var path = (new Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            var directory = System.IO.Path.GetDirectoryName(path);
            return System.IO.Path.Combine(directory, "Chinook_Sqlite.sqlite");
        }

        string _count;
        public string Count
        {
            get { return _count; }
            set { this.RaiseAndSetIfChanged(ref _count, value); }
        }
    }
}

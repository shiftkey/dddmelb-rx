using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SQLite;
using System.Threading;

namespace SearchDemo
{
    public partial class MainWindow
    {
        // this is not MVVM, feel free to mail me the hatorade
        readonly ObservableCollection<Album> _collection = new ObservableCollection<Album>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Items.ItemsSource = _collection;

            // old and busted way
            SearchTextBlocking.TextChanged += (sender1, textChangedEventArgs) =>
            {
                Debug.WriteLine("blocking started at {0}", Environment.TickCount);

                Dispatcher.Invoke(() => _collection.Clear());

                var items = SearchFor(SearchTextBlocking.Text).ToList();

                Dispatcher.Invoke(() =>
                {
                    foreach (var item in items)
                        _collection.Add(item);

                    Count.Text = string.Format("Found {0} results", items.Count);
                });
            };

            // new way
            // turn the TextChanged events into a collection
            var textChangedObservable = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                handler => SearchTextReactive.TextChanged += handler,
                handler => SearchTextReactive.TextChanged -= handler);

            // TODO: this maniac types too fast! halp!            
            textChangedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(next =>
                {
                    // let me know when it starts
                    Debug.WriteLine("Async started at {0}", Environment.TickCount);
                    _collection.Clear();

                    var source = next.Sender as TextBox;
                    var text = source.Text;

                    SearchFor(text).ToObservable() 
                        // ensure this runs on the main thread
                        .ObserveOn(SynchronizationContext.Current)
                        // Finally will run whether the observable collection completes or errors
                        .Finally(() => { Count.Text = string.Format("Found {0} results", _collection.Count); })
                        // with each item, add it to the collection
                        .Subscribe(_collection.Add);
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

        public static string GetDatabasePath()
        {
            var path = (new Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            var directory = System.IO.Path.GetDirectoryName(path);
            var dbPath = System.IO.Path.Combine(directory, "Chinook_Sqlite.sqlite");
            return dbPath;
        }
    }

    [Table("Album")]
    public class Album
    {
        [PrimaryKey, AutoIncrement]
        public int AlbumId { get; set; }

        [MaxLength(160)]
        public string Title { get; set; }

        public int ArtistId { get; set; }
    }
}

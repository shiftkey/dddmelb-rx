using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using SQLite;
using System.Threading;

namespace SearchDemo
{
    public partial class MainWindow
    {
        // this is not MVVM, feel free to mail me the hatorade
        readonly ObservableCollection<Album> _collection = new ObservableCollection<Album>();
        // this is slightly better MVVM because i called it a viewmodel
        MainViewModel viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // this is what we display to the user
            Items.ItemsSource = _collection;

            // old and busted way
            SearchTextBlocking.TextChanged += (sender1, textChangedEventArgs) =>
            {
                Debug.WriteLine("blocking started at {0}", Environment.TickCount);

                // clean up the list
                Dispatcher.Invoke(() => _collection.Clear());

                // get the data
                var items = SearchFor(SearchTextBlocking.Text).ToList();

                // and display the results
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

            var textObservable = textChangedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Select(next =>
                {
                    // what is the text at this point in time?
                    var source = next.Sender as TextBox;
                    var text = source.Text;
                    return text;
                });

            viewModel.WhenTextChanged(textObservable);
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

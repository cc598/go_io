using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Newtonsoft.Json;
using RemindMe.Models;
using RemindMe.ViewModels;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;
using SQLitePCL;

namespace RemindMe
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Todos : Page
    {
        private TodoItemViewModel ViewModel = Common.ViewModel;
        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
        private SQLiteConnection conn = App.conn;
        private string sharetitle = "";
        private string sharedetail = "";
        private string shareimgname = "";
        private string sharedate;
        private string sharetime;
        private StorageFile shareimg;
        class myItem
        {
            public long ID;
            public DateTime date;
            public TimeSpan time;
            public string imgname;
            public string title;
            public string details;
            public bool? finish;
            public myItem(long ID, DateTime date, TimeSpan time, string imgname, string title, string details, bool? finish)
            {
                this.ID = ID;
                this.date = date;
                this.time = time;
                this.imgname = imgname;
                this.title = title;
                this.details = details;
                this.finish = finish;
            }
        }

        public static Todos Current { get; internal set; }

        public Todos()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
        }

        private void DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            Debug.WriteLine(e.Request.ToString());
            DataRequest request = e.Request;
            DataPackage requestData = request.Data;
            requestData.Properties.Title = sharetitle;
            requestData.SetText(sharedetail + sharedate);
            DataRequestDeferral deferral = request.GetDeferral();
            try
            {
                requestData.SetBitmap(RandomAccessStreamReference.CreateFromFile(shareimg));
            }
            finally
            {
                deferral.Complete();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            dataTransferManager.DataRequested -= DataRequested;
            if (((App)Application.Current).IsSuspending)
            {
                ApplicationDataContainer Item = ApplicationData.Current.LocalSettings.CreateContainer("Item", ApplicationDataCreateDisposition.Always);
                if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("Item"))
                {
                    Item.Values["imgname"] = Common.selectName;
                    Item.Values["title"] = title.Text;
                    Item.Values["details"] = details.Text;
                    Item.Values["date"] = date.Date;
                    Item.Values["time"] = time.Time;
                    Item.Values["btn"] = createButton.Content;
                }
                if (ViewModel.SelectedItem != null)
                {
                    ApplicationData.Current.LocalSettings.Values["selectitem"] = ViewModel.getItems.IndexOf(ViewModel.SelectedItem);
                }
                List<string> L = new List<string>();
                var allitems = ViewModel.getItems;
                foreach (var a in allitems)
                {
                    var item = new myItem(a.ID, a.date, a.time, a.imgname, a.title, a.details, a.finish);
                    L.Add(JsonConvert.SerializeObject(item));
                }
                ApplicationData.Current.LocalSettings.Values["allitems"] = JsonConvert.SerializeObject(L);
            }
            //离开页面前更新磁贴
            try
            {
                var AllItems = ViewModel.getItems;
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
                for (var j = AllItems.Count - 1; j >= 0 && j > AllItems.Count - 6; j--)
                {
                    var n = AllItems[j];
                    XmlDocument tile = new XmlDocument();
                    tile.LoadXml(File.ReadAllText("Tile.xml"));
                    XmlNodeList tileText = tile.GetElementsByTagName("text");
                    for (int i = 0; i < tileText.Count; i++)
                    {
                        ((XmlElement)tileText[i]).InnerText = n.title;
                        i++;
                        ((XmlElement)tileText[i]).InnerText = n.details;
                    }
                    TileNotification notification = new TileNotification(tile);
                    updater.Update(notification);
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.ToString());
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            dataTransferManager.DataRequested += DataRequested;
            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("Item");
                ApplicationData.Current.LocalSettings.Values.Remove("allitems");
                ApplicationData.Current.LocalSettings.Values.Remove("selectitem");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("allitems"))
                {
                    ViewModel.getItems.Clear();
                    List<string> L = JsonConvert.DeserializeObject<List<string>>((string)ApplicationData.Current.LocalSettings.Values["allitems"]);
                    foreach (var l in L)
                    {
                        myItem a = JsonConvert.DeserializeObject<myItem>(l);
                        TodoItem item = new TodoItem(a.ID, a.date.Date, a.time , a.imgname, a.title, a.details, a.finish);
                        ViewModel.getItems.Add(item);
                    }
                }
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("selectitem"))
                {
                    ViewModel.SelectedItem = ViewModel.getItems[(int)(ApplicationData.Current.LocalSettings.Values["selectitem"])];
                }

                if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("Item"))
                {
                    ApplicationDataContainer Item = ApplicationData.Current.LocalSettings.Containers["Item"];
                    createButton.Content = Item.Values["btn"] as string;
                    title.Text = Item.Values["title"] as string;
                    details.Text = Item.Values["details"] as string;
                    date.Date = (DateTimeOffset)(Item.Values["date"]);
                    time.Time = (TimeSpan)(Item.Values["time"]);
                    Common.selectName = Item.Values["imgname"] as string;
                    if (Common.selectName == "")
                    {
                        pic.Source = new BitmapImage(new Uri("ms-appx:///Assets/fruit.jpg"));
                    }
                    else
                    {
                        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(Common.selectName);
                        IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                        BitmapImage bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(fileStream);
                        pic.Source = bitmapImage;
                    }
                }
            }
        }

        private async void search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var text = args.QueryText.Trim();
            if (text == "")
                return;
            string alert = "";
            try
            {
                var sql = "SELECT date, time, title, details FROM Todo WHERE date LIKE ? OR time LIKE ? OR title LIKE ? OR details LIKE ?";
                using (var statement = conn.Prepare(sql))
                {
                    statement.Bind(1, "%%" + text + "%%");
                    statement.Bind(2, "%%" + text + "%%");
                    statement.Bind(3, "%%" + text + "%%");
                    statement.Bind(4, "%%" + text + "%%");
                    while (SQLiteResult.ROW == statement.Step())
                    {
                        var date = statement[0].ToString();
                        date = date.Substring(0, date.IndexOf(' '));
                        var time = statement[1].ToString();
                        time = time.Substring(0, date.IndexOf(' '));
                        string title = statement[2] as string;
                        string details = statement[3] as string;
                        alert += "Title: " + title + ";\nDetails: " + details + ";\nDue Date: " + statement[0].ToString() + ";\nDue Time: " + statement[1].ToString() + "\n\n";
                    }
                    if (alert == "")
                        alert = "No result!\n";
                    await new MessageDialog(alert).ShowAsync();
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.ToString());
            }
        }

        private async void share_Click(object sender, RoutedEventArgs e)
        {
            var dc = (sender as FrameworkElement).DataContext;
            var item = (ToDoListView.ContainerFromItem(dc) as ListViewItem).Content as TodoItem;
            sharetitle = item.title;
            sharedetail = item.details;
            shareimgname = item.imgname;
            var date = item.date;
            sharedate = "\nDue date: " + date.Year + '-' + date.Month + '-' + date.Day + " " + time.Time;
            if (shareimgname == "")
            {
                shareimg = await Package.Current.InstalledLocation.GetFileAsync("Assets\\fruit.jpg");
            }
            else
            {
                shareimg = await ApplicationData.Current.LocalFolder.GetFileAsync(shareimgname);
            }
            DataTransferManager.ShowShareUI();
        }

        private void itemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedItem = e.ClickedItem as Models.TodoItem;
            Common.selectName = ViewModel.SelectedItem.imgname;
            if (InlineToDoItemViewGrid.Visibility.ToString() == "Collapsed")
            {
                Frame.Navigate(typeof(Detail));
            }
            else
            {
                delete.Visibility = Visibility.Visible;
                createButton.Content = "Update";
                title.Text = ViewModel.SelectedItem.title;
                details.Text = ViewModel.SelectedItem.details;
                date.Date = ViewModel.SelectedItem.date;
                time.Time = ViewModel.SelectedItem.time;
                pic.Source = ViewModel.SelectedItem.img;
            }
        }

        private void checkBox(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            Line line = VisualTreeHelper.GetChild(parent, 3) as Line;
            line.Opacity = 1;
            try
            {
                var dc = (sender as FrameworkElement).DataContext;
                var listitem = ToDoListView.ContainerFromItem(dc) as ListViewItem;
                var item = listitem.Content as TodoItem;
                string sql = @"UPDATE Todo SET finish = ? WHERE ID = ?";
                using (var res = conn.Prepare(sql))
                {
                    res.Bind(1, "true");
                    res.Bind(2, item.ID);
                    res.Step();
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
            }
        }

        private void uncheckBox(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            Line line = VisualTreeHelper.GetChild(parent, 3) as Line;
            line.Opacity = 0;
            try
            {
                var dc = (sender as FrameworkElement).DataContext;
                var listitem = ToDoListView.ContainerFromItem(dc) as ListViewItem;
                var item = listitem.Content as TodoItem;
                string sql = @"UPDATE Todo SET finish = ? WHERE ID = ?";
                using (var res = conn.Prepare(sql))
                {
                    res.Bind(1, "false");
                    res.Bind(2, item.ID);
                    res.Step();
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
            }
        }

        private void NewPage_Click(object sender, RoutedEventArgs e)
        {
            Common.selectName = "";
            createButton.Content = "Create";
            CancelButton_Click(null, null);
            if (InlineToDoItemViewGrid.Visibility.ToString() == "Visible") return;
            Frame.Navigate(typeof(Detail));
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            title.Text = "";
            details.Text = "";
            date.Date = DateTime.Now;
            time.Time = DateTime.Now.TimeOfDay;
            RandomAccessStreamReference img = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/fruit.jpg"));
            IRandomAccessStream stream = await img.OpenReadAsync();
            BitmapImage bmp = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
            bmp.SetSource(stream);
            pic.Source = bmp;
        }

        private async void CreatButton_Click(object sender, RoutedEventArgs e)
        {
            if (title.Text == "")
            {
                var i = new MessageDialog("Title can not be empty!").ShowAsync();
            }
            if (details.Text == "")
            {
                var i = new MessageDialog("Detail can not be empty!").ShowAsync();
            }
            if (date.Date.CompareTo(DateTime.Today) < 0)
            {
                var i = new MessageDialog("The due date has passed!").ShowAsync();
            }
            if (date.Date.CompareTo(DateTime.Today) == 0 && time.Time.CompareTo(DateTime.Now.TimeOfDay) < 0)
            {
                var i = new MessageDialog("The due time has passed!").ShowAsync();
            }
            if (title.Text != "" && details.Text != "" && date.Date.CompareTo(DateTime.Today) >= 0)
            {
                if (createButton.Content.ToString() == "Create")
                {
                    try
                    {
                        string sql = @"INSERT INTO Todo (date, time, imgname, title, details, finish) VALUES (?,?,?,?,?,?)";
                        using (var res = conn.Prepare(sql))
                        {
                            res.Bind(1, date.Date.Date.ToString());
                            res.Bind(1, time.Time.ToString());
                            res.Bind(2, Common.selectName);
                            res.Bind(3, title.Text.Trim());
                            res.Bind(4, details.Text.Trim());
                            res.Bind(5, "false");
                            res.Step();
                            this.ViewModel.AddItem(conn.LastInsertRowId(), date.Date.Date, time.Time, Common.selectName, title.Text, details.Text, false);
                            Common.selectName = "";
                            CancelButton_Click(null, null);
                            ViewModel.SelectedItem = null;
                            await new MessageDialog("Create successfully!").ShowAsync();
                        }
                    }
                    catch (Exception err)
                    {
                        Debug.WriteLine(err.Message);
                    }
                }
                else
                {
                    try
                    {
                        string sql = @"UPDATE Todo SET date = ?, time = ?, imgname = ?, title = ?, details = ? WHERE ID = ?";
                        using (var res = conn.Prepare(sql))
                        {
                            res.Bind(1, date.Date.Date.ToString());
                            res.Bind(2, time.Time.ToString());
                            res.Bind(3, Common.selectName);
                            res.Bind(4, title.Text.Trim());
                            res.Bind(5, details.Text.Trim());
                            res.Bind(6, ViewModel.SelectedItem.ID);
                            res.Step();
                            ViewModel.SelectedItem.UpdateItem(date.Date.DateTime, time.Time, Common.selectName, title.Text.Trim(), details.Text.Trim());
                            await new MessageDialog("Update successfully!").ShowAsync();
                        }
                    }
                    catch (Exception err)
                    {
                        Debug.WriteLine(err.Message);
                    }
                }
                //添加toast通知，无论create还是update均创建一个新的通知
                try
                {
                    DateTimeOffset remindTime = new DateTimeOffset(new DateTime(date.Date.Year, date.Date.Month, date.Date.Day) + time.Time);
                    DateTimeOffset nowTime = DateTimeOffset.Now;
                    TimeSpan nowtimespan = new TimeSpan(nowTime.Ticks);
                    TimeSpan remindtimespan = new TimeSpan(remindTime.Ticks);
                    TimeSpan timespan = nowtimespan.Subtract(remindtimespan).Duration();
                    int toastTime = (int)timespan.TotalSeconds;
                    XmlDocument toast = new XmlDocument();
                    toast.LoadXml(File.ReadAllText("Toast.xml"));
                    XmlNodeList tileText = toast.GetElementsByTagName("text");
                    ((XmlElement)tileText[0]).InnerText = title.Text;
                    ((XmlElement)tileText[1]).InnerText = details.Text;
                    ScheduledToastNotification toast3 = new ScheduledToastNotification(toast, DateTimeOffset.Now.AddSeconds(toastTime));
                    ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast3);
                }
                catch (Exception err)
                {
                    Debug.WriteLine(err.Message);
                }
            }
        }

        private async void SelectPictureButton_Click(object sender, RoutedEventArgs e)
        {
            var fop = new FileOpenPicker();
            fop.ViewMode = PickerViewMode.Thumbnail;
            fop.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fop.FileTypeFilter.Add(".jpg");
            fop.FileTypeFilter.Add(".jpeg");
            fop.FileTypeFilter.Add(".png");
            fop.FileTypeFilter.Add(".gif");

            StorageFile file = await fop.PickSingleFileAsync();
            try
            {
                IRandomAccessStream fileStream;
                using (fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);
                    pic.Source = bitmapImage;
                    var name = file.Path.Substring(file.Path.LastIndexOf('\\') + 1);
                    Common.selectName = name;
                    await file.CopyAsync(ApplicationData.Current.LocalFolder, name, NameCollisionOption.ReplaceExisting);
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                return;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            delete.Visibility = Visibility.Collapsed;
            createButton.Content = "Create";
            CancelButton_Click(null, null);
            try
            {
                string sql = @"DELETE FROM Todo WHERE ID = ?";
                using (var res = conn.Prepare(sql))
                {
                    res.Bind(1, ViewModel.SelectedItem.ID);
                    res.Step();
                    ViewModel.getItems.Remove(ViewModel.SelectedItem);
                    ViewModel.SelectedItem = null;
                    Common.selectName = "";
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
            }
        }
    }
}
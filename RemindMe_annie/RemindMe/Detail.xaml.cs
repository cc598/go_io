using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using RemindMe.ViewModels;
using System.Collections.Generic;
using Newtonsoft.Json;
using RemindMe.Models;
using SQLitePCL;
using Windows.ApplicationModel.DataTransfer;
using System.IO;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace RemindMe
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Detail : Page
    {
        private TodoItemViewModel ViewModel;

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

        public Detail()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // 保存数据
            if (((App)Application.Current).IsSuspending)
            {
                ApplicationDataContainer Item = ApplicationData.Current.LocalSettings.CreateContainer("Item", ApplicationDataCreateDisposition.Always);
                if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("Item"))
                {
                    Item.Values["title"] = title.Text;
                    Item.Values["details"] = details.Text;
                    Item.Values["date"] = date.Date;
                    Item.Values["time"] = time.Time;
                    Item.Values["imgname"] = Common.selectName;
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
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // 尝试恢复
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            ViewModel = ((ViewModels.TodoItemViewModel)e.Parameter);

            if (e.NavigationMode == NavigationMode.New)
            {
                if (ViewModel.SelectedItem != null)
                {
                    createButton.Content = "Update";
                    title.Text = ViewModel.SelectedItem.title;
                    details.Text = ViewModel.SelectedItem.details;
                    date.Date = ViewModel.SelectedItem.date;
                    time.Time = ViewModel.SelectedItem.time;
                    pic.Source = ViewModel.SelectedItem.img;
                }
                ApplicationData.Current.LocalSettings.Values.Remove("Item");
                ApplicationData.Current.LocalSettings.Values.Remove("allitems");
                ApplicationData.Current.LocalSettings.Values.Remove("selectitem");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("allitems"))
                {
                    var allitems = ViewModel.getItems;
                    allitems.Clear();
                    List<string> L = JsonConvert.DeserializeObject<List<string>>(
                      (string)ApplicationData.Current.LocalSettings.Values["allitems"]);
                    foreach (var l in L)
                    {
                        myItem a = JsonConvert.DeserializeObject<myItem>(l);
                        TodoItem item = new TodoItem(a.ID, a.date.Date, a.time, a.imgname, a.title, a.details, a.finish);
                        allitems.Add(item);
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

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            title.Text = "";
            details.Text = "";
            Common.selectName = "";
            date.Date = System.DateTime.Now;
            time.Time = DateTime.Now.TimeOfDay;
            RandomAccessStreamReference img = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/fruit.jpg"));
            IRandomAccessStream stream = await img.OpenReadAsync();
            BitmapImage bmp = new BitmapImage();
            bmp.SetSource(stream);
            pic.Source = bmp;
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

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Common.selectName = "";
            Frame.Navigate(typeof(Todos));
        }
    }
}

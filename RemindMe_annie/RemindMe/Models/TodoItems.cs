using System;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace RemindMe.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _title;
        private ImageSource _img;
        private string _imgname;
        public long ID { get; set; }
        public string details { get; set; }
        public DateTime date { get; set; }
        public TimeSpan time { get; set; }
        public bool? finish { get; set; }

        public TodoItem(string title, string description, DateTime duedate)
        {
            this.title = title;
            this.details = description;
            this.date = duedate;
        }

        public TodoItem(long ID, DateTime date, TimeSpan time, string imgname, string title = "", string details = "", bool? finish = false)
        {
            this.ID = ID;
            this.date = date;
            this.time = time;
            this.imgname = imgname;
            this.title = title;
            this.details = details;
            this.finish = finish;
            this.setImg();
        }

        public async void setImg()
        {
            if (imgname == "")
            {
                this.img = new BitmapImage(new Uri("ms-appx:///Assets/fruit.jpg"));
            }
            else
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(imgname);
                IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                this.img = bitmapImage;
            }
        }

        private void NotifyPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }

        public string title
        {
            set
            {
                _title = value;
                NotifyPropertyChanged("title");
            }
            get
            {
                return _title;
            }
        }

        public ImageSource img
        {
            set
            {
                _img = value;
                NotifyPropertyChanged("img");
            }
            get
            {
                return _img;
            }
        }

        public string imgname
        {
            set
            {
                _imgname = value;
                NotifyPropertyChanged("imgname");
                setImg();
            }
            get
            {
                return _imgname;
            }
        }

        public void UpdateItem(DateTime date, TimeSpan time, string imgname, string title, string details)
        {
            this.title = title;
            this.details = details;
            this.date = date;
            this.time = time;
            if (this.imgname != imgname)
            {
                this.imgname = imgname;
                this.setImg();
            }
        }
    }
}

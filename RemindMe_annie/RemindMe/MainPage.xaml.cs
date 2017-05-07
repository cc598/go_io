using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using RemindMe.ViewModels;
using RemindMe.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using RemindMe;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace RemindMe
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }

        private TodoItemViewModel ViewModel = Common.ViewModel;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 600));
            if (e.Parameter.GetType() == typeof(ViewModels.TodoItemViewModel))
            {
                this.ViewModel = (ViewModels.TodoItemViewModel)(e.Parameter);
            }
        }
        

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 800)
            {
                Frame.Navigate(typeof(Detail), ViewModel);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            Line line = VisualTreeHelper.GetChild(parent, 3) as Line;
            line.Opacity = (line.Opacity == 1) ? 0 : 1;
        }

        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            // 这个很厉害，可以直接找到这个FlyoutButton的对象是谁
            dynamic o = e.OriginalSource;
            ViewModel.SelectedItem = (TodoItem)o.DataContext;
            if (ViewModel.SelectedItem != null)
            {
                this.ViewModel.RemoveTodoItem(ViewModel.SelectedItem);
                Frame.Navigate(typeof(MainPage), ViewModel);
            }
        }

        private void Edit_Clicked(object sender, RoutedEventArgs e)
        {
            dynamic o = e.OriginalSource;
            ViewModel.SelectedItem = (TodoItem)o.DataContext;
            Frame.Navigate(typeof(Detail), ViewModel);
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text != "")
            {
                Models.TodoItem todo = GetTodo(SearchBox.Text);
                if (todo == null)
                {
                    var i = new MessageDialog("No result!").ShowAsync();
                } else
                {
                    StringBuilder MyStringBuilder = new StringBuilder();
                    MyStringBuilder.Append("Title: ");
                    MyStringBuilder.Append(todo.title);
                    MyStringBuilder.Append(" Description: ");
                    MyStringBuilder.Append(todo.details);
                    MyStringBuilder.Append(" Time: ");
                    MyStringBuilder.Append(todo.date.ToString());
                    var i = new MessageDialog(MyStringBuilder.ToString()).ShowAsync();
                }
            }
        }

        private Models.TodoItem GetTodo(string to_select_title)
        {
            var db = App.conn;
            Models.TodoItem todo = null;
            using (var statement = db.Prepare("SELECT title, details, date FROM Todo WHERE Title = ?"))
            {
                statement.Bind(1, to_select_title);
                SQLiteResult result = statement.Step();
                if (SQLiteResult.ROW == result)
                {
                    todo = new TodoItem((string)statement[0], (string)statement[1], Convert.ToDateTime((string)statement[2]));
                }
            }
            return todo;
        }

         private void CalendarView_CalendarViewDayItemChanging(CalendarView sender,
                                    CalendarViewDayItemChangingEventArgs args)
         {
             // Render basic day items.
             if (args.Phase == 0)
             {
                 // Register callback for next phase.
                 args.RegisterUpdateCallback(CalendarView_CalendarViewDayItemChanging);
             }
             // Set blackout dates.
             else if (args.Phase == 1)
             {
                 // Blackout dates in the past, Sundays, and dates that are fully booked.
                 if (args.Item.Date < DateTimeOffset.Now)
                 {
                     args.Item.IsBlackout = true;
                 }
                 // Register callback for next phase.
                 args.RegisterUpdateCallback(CalendarView_CalendarViewDayItemChanging);
             }
             // Set density bars.
             else if (args.Phase == 2)
             {
                 // Avoid unnecessary processing.
                 // You don't need to set bars on past dates or Sundays.
                 if (args.Item.Date > DateTimeOffset.Now &&
                     args.Item.Date.DayOfWeek != DayOfWeek.Sunday)
                 {
                     // Get bookings for the date being rendered.
                     string str1 = args.Item.Date.ToString();
                     DateTime dt1 = Convert.ToDateTime(str1);
                     var currentItem = ViewModel.GetItems(dt1);

                     List<Color> densityColors = new List<Color>();
                     // Set a density bar color for each of the days bookings.
                     // It's assumed that there can't be more than 10 bookings in a day. Otherwise,
                     // further processing is needed to fit within the max of 10 density bars.
                     foreach (var Item in currentItem)
                     {
                         if (Item.finish == true)
                         {
                             densityColors.Add(Colors.Green);
                         }
                         else
                         {
                             densityColors.Add(Colors.Blue);
                         }
                     }
                     args.Item.SetDensityColors(densityColors);
                 }
             }


         }
    }
}

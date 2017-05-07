using RemindMe.Models;
using SQLitePCL;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RemindMe.ViewModels
{
    class TodoItemViewModel
    {
        private ObservableCollection<Models.TodoItem> items = new ObservableCollection<Models.TodoItem>();
        public ObservableCollection<Models.TodoItem> AllItems { get { return this.items; } }


        private TodoItem selectedItem;
        public TodoItem SelectedItem
        {
            get { return selectedItem; }
            set { this.selectedItem = value; }
        }

        public TodoItemViewModel()
        {
            try
            {
                var sql = "SELECT * FROM Todo";
                var conn = App.conn;
                using (var statement = conn.Prepare(sql))
                {
                    while (SQLiteResult.ROW == statement.Step())
                    {
                        var s = statement[1].ToString();
                        s = s.Substring(0, s.IndexOf(' '));
                        long ID = (long)statement[0];
                        DateTime date = new DateTime(int.Parse(s.Split('/')[0]), int.Parse(s.Split('/')[1]), int.Parse(s.Split('/')[2]));
                        var t = statement[2].ToString();
                        t = t.Substring(0, t.IndexOf(' '));
                        TimeSpan time = new TimeSpan(int.Parse(s.Split(':')[0]), int.Parse(s.Split(':')[1]), int.Parse(s.Split(':')[2]));
                        string imgname = (string)statement[3];
                        string title = (string)statement[4];
                        string details = (string)statement[5];
                        bool finish = Boolean.Parse(statement[6] as string);
                        this.AddItem(ID, date, time, imgname, title, details, finish);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
            }
        }

        public ObservableCollection<Models.TodoItem> getItems
        {
            get { return this.items; }
        }

        public void AddItem(long ID, DateTime date, TimeSpan time, string imgname, string title, string details, bool? finish)
        {
            items.Add(new Models.TodoItem(ID, date, time, imgname, title, details, finish));
        }

        public ObservableCollection<Models.TodoItem> GetItems(DateTime date)
        {
            ObservableCollection<Models.TodoItem> result = new ObservableCollection<Models.TodoItem>();
            foreach (var i in items)
            {
                if (i.date == date)
                {
                    result.Add(i);
                }
            }
            return result;
        }
        
        public void RemoveTodoItem(Models.TodoItem param)
        {
            // DIY
            this.items.Remove(this.selectedItem);
            // 从数据库中把记录删除
            var db = App.conn;
            try
            {
                using (var todo = db.Prepare("DELETE FROM Todo WHERE Id = ?"))
                {
                    todo.Bind(1, this.selectedItem.ID);
                    todo.Step();
                }
            }
            catch (Exception ex)
            {
                // TODO: Handle error
            }

            // set selectedItem to null after remove
            this.selectedItem = null;
        }
    }
}

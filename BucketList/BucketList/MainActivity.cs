﻿using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;
using Android.Widget;
using Java.Lang;
using System.Linq;
using AndroidX.Core.Util;
using Android.Content;
using System.IO;
using Android.Text;
using Google.Android.Material.Resources;
using Android.Text.Style;
using Android.Util;
using Json.Net;
using AlertDialog = Android.App.AlertDialog;

namespace BucketList
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        public List<Goal> Goals;
        public List<DatePythonCalendar> datesPythonCalendar;
        private User user;
        private string userName;
        private Goal currentGoalName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Initialize();
            SetTitle(Resource.String.empty_string);
            SetContentView(Resource.Layout.activity_main);
            SetListView();
            SetNavigationView();
            SetUserName();
            SetFab();
            SetToolbar();
            SetPythonCalendarView();
            SetSearchView();
            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            var failedGoals = GetFailedGoals();
            user.UserStatistics.GoalsFailedCount += failedGoals.Count;
            Extensions.OverwriteUser(user);
        }

        private List<Goal> GetFailedGoals()
        {
            return Goals
                    .Where(x => x.Deadline < DateTime.Now)
                    .ToList();
        }

        private void SetSearchView()
        {
            var searchView = FindViewById<Android.Widget.SearchView>(Resource.Id.main_screen_search);
            searchView.QueryTextChange += SearchView_QueryTextChange;
        }

        private void SearchView_QueryTextChange(object sender, Android.Widget.SearchView.QueryTextChangeEventArgs e)
        {
            var searchView = sender as Android.Widget.SearchView;
            var text = searchView.Query.ToLower().Trim();
            var goals =
                Goals
                .Select(x => x.GoalName)
                .Where(x => x.ToLower().Contains(text))
                .ToList();
            UpdateGoalsViewForSearchView(goals);
        }

        private void Initialize()
        {
            datesPythonCalendar = new List<DatePythonCalendar>();
            if (string.IsNullOrEmpty(Extensions.ReadGoals()))
                Extensions.OverwriteGoals(Extensions.SerializeGoals(new List<Goal>()));
            Goals = Extensions.GetSavedGoals();
            user = Extensions.GetSavedUser();
        }

        private void MyListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            // Получите ссылку на ListView
            ListView myListView = sender as ListView;

            // Получите выбранный элемент
            var selectedItem = (string)myListView.GetItemAtPosition(e.Position);
            currentGoalName = Goals.First(x => x.GoalName == selectedItem);

            // Отобразите контекстное меню
            RegisterForContextMenu(myListView);

            // Откройте контекстное меню для выбранного элемента
            OpenContextMenu(myListView);
        }

        public override void OnCreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, view, menuInfo);

            // Меню для цели
            if (view is ListView)
            {
                menu.SetHeaderTitle("Удалить цель?");

                menu.Add(Menu.None, 0, Menu.None, "Да");
                menu.Add(Menu.None, 1, Menu.None, "Нет");
            }

            // Меню для даты в календаре с питоном
            else if (view is TextView)
            {

            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            if (item.ItemId == 0)
            {
                RemoveGoal(currentGoalName);
                return true;
            }

            return base.OnContextItemSelected(item);
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                //base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(this, typeof(AddGoalActivity));
            StartActivityForResult(intent, 1);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var id = item.ItemId;

            if (id == Resource.Id.nav_statistics)
            {
                var intent = new Intent(this, typeof(StatisticsActivity));
                StartActivityForResult(intent, 1);
            }
            else if (id == Resource.Id.nav_account)
            {

            }
            else if (id == Resource.Id.nav_python_settings)
            {

            }
            else if (id == Resource.Id.nav_allow_notifications)
            {

            }
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok && data != null)
            {
                var newGoal = JsonNet.Deserialize<Goal>(data.GetStringExtra("goal"));
                AddGoal(newGoal);
            }
        }

        private void AddGoal(Goal goal)
        {
            Goals.Add(goal);
            foreach (var date in datesPythonCalendar)
            {
                SetDeadlineDate(goal, date);
            }
            user.UserStatistics.GoalsCreatedCount++;
            Extensions.OverwriteUser(user);
            UpdateGoalsView();
        }

        private void RemoveGoal(Goal goal)
        {
            Extensions.DeleteImage(goal.ImagePath);
            Goals.Remove(goal);
            foreach (var date in datesPythonCalendar)
            {
                DeleteDeadlineFromDate(goal, date);
            }
            user.UserStatistics.GoalsDeletedCount++;
            Extensions.OverwriteUser(user);
            UpdateGoalsView();
        }

        private void UpdateGoalsView()
        {
            var serializedGoals = Extensions.SerializeGoals(Goals);
            Extensions.OverwriteGoals(serializedGoals);
            var listView = FindViewById<ListView>(Resource.Id.goalsListView);
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.all_goals_list_item, Goals.Select(x => x.GoalName).ToList());
            listView.Adapter = adapter;
        }

        private void UpdateGoalsViewForSearchView(List<string> goals)
        {
            var listView = FindViewById<ListView>(Resource.Id.goalsListView);
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.all_goals_list_item, goals);
            listView.Adapter = adapter;
        }

        private void OnGoalClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Получите ссылку на ListView
            ListView myListView = sender as ListView;

            // Получите выбранный элемент
            var selectedItem = (string)myListView.GetItemAtPosition(e.Position);
            Intent intent = new Intent(this, typeof(GoalActivity));
            intent.PutExtra("goal", JsonNet.Serialize(Goals.First(x => x.GoalName == selectedItem)));
            StartActivityForResult(intent, 1);
        }

        private void SetNavigationView()
        {
            var user = Extensions.GetSavedUser();
            userName = user.UserName;
            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            var headerView = navigationView.GetHeaderView(0);
            var userImage = headerView.FindViewById<ImageView>(Resource.Id.imageView);
            userImage.SetImage(user.UserPhotoPath);
            navigationView.SetNavigationItemSelectedListener(this);
        }

        private void SetUserName()
        {
            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            var headerView = navigationView.GetHeaderView(0);
            var usernameTextView = headerView.FindViewById<TextView>(Resource.Id.usernameMainTextView);
            if (!string.IsNullOrEmpty(userName))
            {
                usernameTextView.Text = userName;
            }
        }

        private void SetListView()
        {
            var listView = FindViewById<ListView>(Resource.Id.goalsListView);
            UpdateGoalsView();
            listView.ItemClick += OnGoalClick;
            listView.ItemLongClick += MyListView_ItemLongClick;
        }

        private void SetFab()
        {
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
        }

        private void SetToolbar()
        {
            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();
        }

        private void SetPythonCalendarView()
        {
            var calendar = FindViewById<RelativeLayout>(Resource.Id.deadlineCalendar);
            var buttonCalendarOpen = FindViewById<Button>(Resource.Id.calendarButton);
            buttonCalendarOpen.Click += ButtonCalendarOpen_Click;

            var currentDateTime = DateTime.Now.AddDays(-3);
            
            for (var i = 0; i < calendar.ChildCount; i++)
            {
                // Дата - это TextView в Layout
                var dateView = calendar.GetChildAt(i) as TextView;
                if (dateView == null) return;
                var date = new DatePythonCalendar(currentDateTime, dateView);
                dateView.Text = currentDateTime.Day.ToString();
                datesPythonCalendar.Add(date);
                foreach (var goal in Goals)
                {
                    SetDeadlineDate(goal, date);
                }
                // Изменяем текущий день на следующий
                currentDateTime = currentDateTime.AddDays(1);
            }
        }

        private void ButtonCalendarOpen_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(CalendarActivity));
            intent.PutExtra("goals", Extensions.SerializeGoals(Goals));
            StartActivity(intent);
        }

        private void SetDeadlineDate(Goal goal, DatePythonCalendar date)
        {
            if (date.Deadline.Date == goal.Deadline.Date)
            {
                date.Goal = goal;
                date.View.Tag = date;
                date.View.Background = GetDrawable(Resource.Drawable.deadlineMouse1);
            }
        }
        private void DeleteDeadlineFromDate(Goal goal, DatePythonCalendar date)
        {
            if (goal.Deadline.Date == date.Deadline.Date)
            {
                date.Goal = null;
                date.View.Background = GetDrawable(Resource.Drawable.dateInCalendarWithPython);
            }
        }
    }
}




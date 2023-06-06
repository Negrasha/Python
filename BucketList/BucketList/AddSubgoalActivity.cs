﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using Java.Util.Zip;
using Json.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace BucketList
{
    [Activity(Label = "AddSubgoalActivity")]
    public class AddSubgoalActivity : Activity
    {
        Button goalAddButton;
        EditText goalAddNameEditText;
        ImageView snake;
        DatePickerDialog datePickerDialog;

        private CalendarView calendarView;
        private DateTime selectedDate;

        private List<Goal> allGoals;
        private Goal currentGoal;
        private DateTime maxDeadline;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_add_goal);
            goalAddButton = FindViewById<Button>(Resource.Id.goal_add_button);
            goalAddButton.Click += OnClick;
            allGoals = GoalExtensions.GetSavedGoals();
            currentGoal = GoalExtensions.DeserializeGoal(Intent.GetStringExtra("currentGoal"));
            maxDeadline = DateTime.Parse(Intent.GetStringExtra("maxDeadline"));
            goalAddNameEditText = FindViewById<EditText>(Resource.Id.goal_add_name_text);
            
            var datePickerButton = FindViewById<Button>(Resource.Id.button1);
            datePickerButton.Click += DatePickerButton_Click;
        }

        private void OnClick(object sender, EventArgs e)
        {
            var intent = new Intent();
            var goalName = GetGoalNameText();
            if (currentGoal.Subgoals.Any(x => x.SubgoalName == goalName))
            {
                Toast.MakeText(this, "Подцель с таким названием уже существует", ToastLength.Short).Show();
                return;
            }
            if (DateTime.Compare(GetGoalDateTime(), DateTime.Now) <= 0)
            {
                Toast.MakeText(this, "Вы не выбрали дедлайн", ToastLength.Short).Show();
                return;
            }
            var goalDeadline = GetGoalDateTime();
            var goal = new Goal(goalName, goalDeadline);
            intent.PutExtra("goal", JsonNet.Serialize(goal));
            SetResult(Result.Ok, intent);
            Finish();
        }

        private string GetGoalNameText()
        {
            var text = goalAddNameEditText.Text;
            if (text == "") return "Новая цель";
            return text;
        }

        private DateTime GetGoalDateTime()
        {
            return selectedDate;
        }

        private void DatePickerButton_Click(object sender, EventArgs e)
        {
            var currentDate = DateTime.Now;

            LayoutInflater inflater = LayoutInflater.From(this);
            View datePickerView = inflater.Inflate(Resource.Layout.calendar, null);
            CalendarView calendarView = datePickerView.FindViewById<CalendarView>(Resource.Id.addGoalCalendar);
            this.calendarView = calendarView;
            DatePickerDialog datePickerDialog = new DatePickerDialog(
                                                this,
                                                OnDateSet,
                                                currentDate.Year,
                                                currentDate.Month - 1,
                                                currentDate.Day
                                                );
            datePickerDialog.SetView(datePickerView);
            calendarView.MinDate = currentDate.GetDateTimeInMillis();
            calendarView.MaxDate = maxDeadline.GetDateTimeInMillis();
            calendarView.FirstDayOfWeek = 2;
            calendarView.DateChange += CalendarView_DateChange;
            datePickerDialog.Show();

            //datePickerDialog
            //    = new DatePickerDialog(
            //                        this,
            //                        OnDateSet,
            //                        currentDate.Year,
            //                        currentDate.Month - 1,
            //                        currentDate.Day
            //                        );
            //datePickerDialog.DatePicker.MinDate = DateTime.Now.GetDateTimeInMillis();
            //datePickerDialog.SetContentView(Resource.Layout.calendar);
            //datePickerDialog.Show();

            //LayoutInflater inflater = LayoutInflater.From(this);
            //View datePickerView = inflater.Inflate(Resource.Layout.calendar, null);
            //DatePickerDialog datePickerDialog = new DatePickerDialog(
            //                                    this,
            //                                    OnDateSet,
            //                                    currentDate.Year,
            //                                    currentDate.Month - 1,
            //                                    currentDate.Day
            //                                    );
            //datePickerDialog.DatePicker.MinDate = DateTime.Now.GetDateTimeInMillis();
            //datePickerDialog.SetView(datePickerView);
            //datePickerDialog.Show();

        }

        private void CalendarView_DateChange(object sender, CalendarView.DateChangeEventArgs args)
        {
            var selectedDate = new DateTime(args.Year, args.Month + 1, args.DayOfMonth);
            this.selectedDate = selectedDate;
        }

        private void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            var year = selectedDate.Year;
            var month = selectedDate.Month; // Здесь месяц начинается с 0, поэтому добавляем 1
            var dayOfMonth = selectedDate.Day;
            var selectedDateInString = $"{dayOfMonth}-{month}-{year}";
            Toast.MakeText(this, $"Выбранная дата: {selectedDateInString}", ToastLength.Short).Show();
            //var calendarView = datePicker.RootView.FindViewById<CalendarView>(Resource.Id.addGoalCalendar);
            //var selectedDate = calendarView.Date.GetDateTimeFromMillis();
            //var year = selectedDate.Year;
            //var month = selectedDate.Month + 1; // Здесь месяц начинается с 0, поэтому добавляем 1
            //var dayOfMonth = selectedDate.Day;
            //var selectedDateInString = $"{dayOfMonth}-{month}-{year}";
            //Toast.MakeText(this, $"Выбранная дата: {selectedDateInString}", ToastLength.Short).Show();


            //// Получите выбранную дату из DatePickerDialog
            //int year = e.Year;
            //int month = e.Month + 1; // Здесь месяц начинается с 0, поэтому добавляем 1
            //int dayOfMonth = e.DayOfMonth;


            //// Выполните необходимые действия с выбранной датой
            //string selectedDate = $"{dayOfMonth}-{month}-{year}";
            //Toast.MakeText(this, $"Выбранная дата: {selectedDate}", ToastLength.Short).Show();
        }
    }
}
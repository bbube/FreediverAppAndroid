﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using FreediverApp.DatabaseConnector;
using FreediverApp.GeoLocationServiceNamespace;

namespace FreediverApp
{
    /**
     *  This fragment contains the view with a list of all divesessions from the current user. It also provides 
     *  a add button to create a new divesession inside a other activity. A divesession inside the listview can be clicked
     *  to call the DiveSessionDetailViewActivity with the data of the clicked divesession.
     **/
    [Obsolete]
    public class DiveSessionsFragment : Fragment
    {
        private List<string> dives;
        private ListView listViewDives;
        private Button buttonAdd;
        private FirestoreDataListener diveSessionsDataListener;
        private List<DiveSession> diveSessionList;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        /**
         *  This function initializes all ui components and returns the view to be displayed in the fragment manager 
         *  of our main activity.
         **/
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.SessionsPage, container, false);

            listViewDives = view.FindViewById<ListView>(Resource.Id.lvwDiveSessions);

            RetrieveDiveSessionData();

            listViewDives.ItemClick += lvwDive_ItemClick;

            buttonAdd = view.FindViewById<Button>(Resource.Id.btnAdd);
            buttonAdd.Click += buttonAdd_Click;

            return view;
        }

        /**
         *  This function initializes the db listener and queries for all divesessions that belong to the current user.
         *  The query gets all divesessions in which the field "ref_user" is set to the id of the current user.
         **/
        private void RetrieveDiveSessionData()
        {
            diveSessionsDataListener = new FirestoreDataListener();
            diveSessionsDataListener.QueryParameterizedOrderBy("divesessions", "ref_user", TemporaryData.CURRENT_USER.id, "date");
            diveSessionsDataListener.DataRetrieved += DiveSessionsDataListener_DataRetrieved;
        }

        /**
         *  This function sets the divesessionlist to the list of retrieved divesessions from the db listener.
         *  After that the listview is populated with the retrieved divesession data.
         **/
        private void DiveSessionsDataListener_DataRetrieved(object sender, FirestoreDataListener.DataEventArgs e)
        {
            diveSessionList = e.DiveSessions;
            fillDiveSessionData(diveSessionList);
        }

        /**
         *  This function populates the listview with the retrieved divesessions that were queried from the db.
         *  A Listview entry contains the date of the divesession and the coordinates of the location where the 
         *  divesession was created.
         **/
        private async void fillDiveSessionData(List<DiveSession> diveSessions)
        {
            //diveSessionList.Sort((x, y) => Convert.ToDateTime(x.date).CompareTo(Convert.ToDateTime(y.date)));
            if (diveSessions != null)
            {
                dives = new List<string>();
                GeoLocationService geoLocationService = new GeoLocationService();
                foreach (var item in diveSessionList)
                {
                    if (item.date != null)
                    {
                        if (!string.IsNullOrEmpty(item.location_lat) || !string.IsNullOrEmpty(item.location_lon))
                        {
                            double location_lat = float.Parse(item.location_lat.ToString().Replace(',', '.'), CultureInfo.GetCultureInfo("en").NumberFormat);
                            double location_lon = float.Parse(item.location_lon.ToString().Replace(',', '.'), CultureInfo.GetCultureInfo("en").NumberFormat);
                            await geoLocationService.getLocation_name(location_lat, location_lon);
                            dives.Add(item.date + " | " + geoLocationService.location_locality);
                        }
                        else
                        {
                            dives.Add(item.date + " | " + "n/a");
                        }
                    }
                }

                ArrayAdapter<string> adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleListItem1, dives);
                listViewDives.Adapter = adapter;
                TemporaryData.CURRENT_USER.diveSessions = diveSessions;
            }
        }

        /**
         *  This function handles the onclick event of the listview. If a divesession was selected inside the listview, 
         *  The current divesession in the TemporaryData class is set to the divesession from the listview and then 
         *  a divesessionDetailActivity is started that reads the date from the current divesession in TemporaryData.
         **/
        async void lvwDive_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            TemporaryData.CURRENT_DIVESESSION = TemporaryData.CURRENT_USER.diveSessions[e.Position];

            if (!String.IsNullOrEmpty(TemporaryData.CURRENT_DIVESESSION.location_lat) || !String.IsNullOrEmpty(TemporaryData.CURRENT_DIVESESSION.location_lon))
            {
                double location_lat = float.Parse(TemporaryData.CURRENT_DIVESESSION.location_lat.ToString().Replace(',', '.'), CultureInfo.GetCultureInfo("en").NumberFormat);
                double location_lon = float.Parse(TemporaryData.CURRENT_DIVESESSION.location_lon.ToString().Replace(',', '.'), CultureInfo.GetCultureInfo("en").NumberFormat);
                GeoLocationService geoLocationSevice = new GeoLocationService();
                try
                {
                    await geoLocationSevice.getLocation_name(location_lat, location_lon);
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp);
                }
                TemporaryData.CURRENT_DIVESESSION.location_locality = geoLocationSevice.location_locality;
            }
            else
            {
                TemporaryData.CURRENT_DIVESESSION.location_locality = "n/a";
            }
        
            var diveSessionDetailViewActivity = new Intent(Context, typeof(DiveSessionDetailViewActivity));
            StartActivity(diveSessionDetailViewActivity);
        }

        /**
         *  This function handles the onclick event of the add button. When the button was clicked a new instance of 
         *  addSessionActivity is started to create a new divesession.
         **/
        void buttonAdd_Click(object sender, EventArgs eventArgs)
        {
            FragmentManager.BeginTransaction().Detach(this).Attach(this).Commit();
            var addSessionActivity = new Intent(Context, typeof(AddSessionActivity));
            StartActivity(addSessionActivity);
        }
    }
}
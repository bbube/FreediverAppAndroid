﻿using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;

//DataListener that handles the retrieval of data from db and returns the data to the activity/fragment by invoking a event
namespace FreediverApp.DatabaseConnector
{
    public class FirebaseDataListener : Java.Lang.Object, IValueEventListener
    {
        //Lists for retrieved data entities that will then be returned to the activity by invoking the dataRetrieved Event
        //If we need a new type of table you should add a new list to handle the retrieved Data for that case
        List<User> userList = new List<User>();
        List<DiveSession> divesessionList = new List<DiveSession>();
        List<Dive> diveList = new List<Dive>();
        List<Measurepoint> measurePointList = new List<Measurepoint>();

        public event EventHandler<DataEventArgs> DataRetrieved;

        public class DataEventArgs : EventArgs 
        {
            internal List<User> Users { get; set; }
            internal List<DiveSession> DiveSessions { get; set; }
            internal List<Measurepoint> Measurepoints { get; set; }
            internal List<Dive> Dives { get; set; }
        }

        public void QueryFullTable(string tablename) 
        {
            DatabaseReference tableRef = DatabaseConnector.GetDatabase().GetReference(tablename);
            tableRef.AddValueEventListener(this);
        }

        public void QueryParameterized(string tablename, string field, string value) 
        {
            DatabaseReference tableRef = DatabaseConnector.GetDatabase().GetReference(tablename);
            tableRef.OrderByChild(field).EqualTo(value).AddValueEventListener(this);
        }

        public void OnCancelled(DatabaseError error)
        {
            
        }

        public void createEntitiesFromSnapshot(DataSnapshot snapshot) 
        {
            //we cannot generalize this completely at the moment -> would be too much since we dont need a full db api for the small amount of tasks
            string tablename = snapshot.Ref.Key;
            var dataResult = snapshot.Children.ToEnumerable<DataSnapshot>();

            //determine which table was queried. If another table needs to be handled the case has to be added to the below switch statement
            switch (tablename)
            {
                case "users":
                {
                    foreach (DataSnapshot dataRecord in dataResult)
                    {
                        User user = new User();
                        user.id = dataRecord.Key;
                        user.username = dataRecord.Child("username").Value.ToString();
                        user.password = dataRecord.Child("password").Value.ToString();
                        user.email = dataRecord.Child("email").Value.ToString();
                        user.firstname = dataRecord.Child("firstname").Value.ToString();
                        user.lastname = dataRecord.Child("lastname").Value.ToString();
                        user.dateOfBirth = dataRecord.Child("birthday").Value.ToString();
                        user.weight = dataRecord.Child("weight").Value.ToString();
                        user.height = dataRecord.Child("height").Value.ToString();
                        user.registerdate = dataRecord.Child("registerdate").Value.ToString();
                        userList.Add(user);
                    }
                    break;
                }
                case "divesessions":
                {
                    foreach (DataSnapshot dataRecord in dataResult)
                    {
                        DiveSession divesession = new DiveSession();
                        divesession.date = dataRecord.Child("date").Value.ToString();
                        divesession.location = dataRecord.Child("location").Value.ToString();
                        divesession.refUser = dataRecord.Child("ref_user").Value.ToString();
                        divesession.watertime = dataRecord.Child("watertime").Value.ToString();
                        divesession.weatherCondition = dataRecord.Child("weather_condition").Value.ToString();
                        divesession.weatherTemperature = dataRecord.Child("weather_temp").Value.ToString();
                        divesession.weatherWindSpeed = dataRecord.Child("weather_wind_speed").Value.ToString();
                        divesession.Id = dataRecord.Child("id").Value.ToString();
                        divesessionList.Add(divesession);
                    }
                    break;
                }
                case "dives":
                {
                    foreach (DataSnapshot dataRecord in dataResult)
                    {
                        Dive dive = new Dive();                        
                        dive.duration = dataRecord.Child("duration").Value.ToString();
                        dive.HeartFreqMax = dataRecord.Child("heart_freq_max").Value.ToString();
                        dive.HeartFreqMin = dataRecord.Child("heart_freq_min").Value.ToString();
                        dive.LuminanceMax = dataRecord.Child("luminance_max").Value.ToString();
                        dive.LuminanceMin = dataRecord.Child("luminance_min").Value.ToString();
                        dive.maxDepth = dataRecord.Child("max_depth").Value.ToString();
                        dive.OxygenSaturationMax = dataRecord.Child("oxygen_saturation_max").Value.ToString();
                        dive.OxygenSaturationMin = dataRecord.Child("oxygen_saturation_min").Value.ToString();                           
                        dive.refDivesession = dataRecord.Child("ref_divesession").Value.ToString();
                        dive.timestampBegin = dataRecord.Child("timestamp_begin").Value.ToString();
                        dive.timestampEnd = dataRecord.Child("timestamp_end").Value.ToString();
                        dive.WaterTemperatureMax = dataRecord.Child("water_temp_max").Value.ToString();
                        dive.WaterTemperatureMin = dataRecord.Child("water_temp_min").Value.ToString();
                        dive.Id = dataRecord.Child("id").Value.ToString();
                        diveList.Add(dive);
                    }
                    break;
                }
                case "measurepoints":
                {
                    foreach (DataSnapshot dataRecord in dataResult)
                    {
                        Measurepoint measurepoint = new Measurepoint();
                        measurepoint.accelerator_x = dataRecord.Child("accelerator_x").Value.ToString();
                        measurepoint.accelerator_y = dataRecord.Child("accelerator_y").Value.ToString();
                        measurepoint.accelerator_z = dataRecord.Child("accelerator_z").Value.ToString();
                        measurepoint.depth = dataRecord.Child("depth").Value.ToString();
                        measurepoint.duration = dataRecord.Child("duration").Value.ToString();
                        measurepoint.gyroscope_x = dataRecord.Child("gyroscope_x").Value.ToString();
                        measurepoint.gyroscope_y = dataRecord.Child("gyroscope_y").Value.ToString();
                        measurepoint.gyroscope_z = dataRecord.Child("gyroscope_z").Value.ToString();
                        measurepoint.heartFreq = dataRecord.Child("heart_freq").Value.ToString();
                        measurepoint.heartVar = dataRecord.Child("heart_var").Value.ToString();
						measurepoint.luminance = dataRecord.Child("luminance").Value.ToString();
                        measurepoint.oxygenSaturation = dataRecord.Child("oxygen_saturation").Value.ToString();
                        measurepoint.refDive = dataRecord.Child("ref_dive").Value.ToString();
                        measurepoint.waterTemperature = dataRecord.Child("water_temp").Value.ToString();
                        measurePointList.Add(measurepoint);
                    }
                    break;
                }
                default: { break; }
            }
        }

        public void deleteEntity(string tablename, string id) 
        {
            DatabaseReference entityRef = DatabaseConnector.GetDatabase().GetReference(tablename + "/" + id);
            entityRef.RemoveValue();
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Value != null)
            {
                //clear all datalists before new retrieved data is loaded from db
                clearDataLists();

                //instantiate a dataobject for every entry that was returned by the query
                createEntitiesFromSnapshot(snapshot);

                //invoke the dataRetrievedEvent for the corresponding table
                invokeDataRetrievedEvent(snapshot);
            }
            else 
            {
                //if no data was returned also invoke the dataRetrievedEvent but set all datalists to null
                DataRetrieved.Invoke(this, new DataEventArgs { Users = null, DiveSessions = null, Dives = null, Measurepoints = null });
            }
        }

        public void invokeDataRetrievedEvent(DataSnapshot snapshot) 
        {
            string tablename = snapshot.Ref.Key;

            if (tablename == "users")
            {
                DataRetrieved.Invoke(this, new DataEventArgs { Users = userList, DiveSessions = null, Dives = null, Measurepoints = null });
            }
            else if (tablename == "divesessions")
            {
                DataRetrieved.Invoke(this, new DataEventArgs { DiveSessions = divesessionList, Users = null, Dives = null, Measurepoints = null });
            }
            else if (tablename == "dives") 
            {
                DataRetrieved.Invoke(this, new DataEventArgs { Dives = diveList, Users = null, DiveSessions = null, Measurepoints = null });
            }
            else if (tablename == "measurepoints")
            {
                DataRetrieved.Invoke(this, new DataEventArgs { Measurepoints = measurePointList, Users = null, DiveSessions = null, Dives = null });
            }
        }

        public void clearDataLists() 
        {
            userList.Clear();
            divesessionList.Clear();
            diveList.Clear();
            measurePointList.Clear();
        }
    }
}
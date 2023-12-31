﻿using System;
using System.Collections.Generic;
using FreediverApp.UI.Fragments;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using FreediverApp.DatabaseConnector;
using SupportV7 = Android.Support.V7.App;
using System.Globalization;

namespace FreediverApp
{
    /**
     *  This Fragment displays the account data of the current user that is signed in. The user also has the possibility to 
     *  delete his account with a button on the bottom of the form. All dive data from that user still remains inside the databse 
     *  after deletion.
     **/
    [Obsolete]
    public class AccountPanelFragment : Fragment
    {
        /*Member Variables (UI Components from XML)*/
        private TextView txtViewEmail, txtViewPassword, txtViewFirstname, txtViewLastname, txtViewDateOfBirth, txtViewHeight, txtViewWeight, txtViewGender;
        private TextView titleUsername, titleRegisteredSince;
        private Button btnDeleteAccount;

        private FirestoreDataListener userDataListener;
        private List<User> userList;

        private string gender;

        // use edit buttons as Imageviews as it is easier and costs less resources
        ImageView btnEditEmail, btnEditPassword, btnEditFirstname, btnEditLastname, btnEditDateOfBirth, btnEditHeight, btnEditWeight, btnEditGender;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.AccountPanelPage, container, false);

            btnDeleteAccount = view.FindViewById<Button>(Resource.Id.button_delete_account);

            btnDeleteAccount.Click += deleteUserAccount;

            txtViewEmail = view.FindViewById<TextView>(Resource.Id.txtview_email);
            txtViewPassword = view.FindViewById<TextView>(Resource.Id.txtview_password);
            txtViewFirstname = view.FindViewById<TextView>(Resource.Id.txtview_firstname);
            txtViewLastname = view.FindViewById<TextView>(Resource.Id.txtview_lastname);
            txtViewDateOfBirth = view.FindViewById<TextView>(Resource.Id.txtview_date_of_birth);
            txtViewHeight = view.FindViewById<TextView>(Resource.Id.txtview_height);
            txtViewWeight = view.FindViewById<TextView>(Resource.Id.txtview_weight);
            txtViewGender = view.FindViewById<TextView>(Resource.Id.txtview_gender);

            titleUsername = view.FindViewById<TextView>(Resource.Id.title_username);
            titleRegisteredSince = view.FindViewById<TextView>(Resource.Id.title_registered_since);

            //Instantiate ImageView pencil Buttons for editing data
            btnEditEmail = view.FindViewById<ImageView>(Resource.Id.btn_edit_email);
            btnEditPassword = view.FindViewById<ImageView>(Resource.Id.btn_edit_password);
            btnEditFirstname = view.FindViewById<ImageView>(Resource.Id.btn_edit_firstname);
            btnEditLastname = view.FindViewById<ImageView>(Resource.Id.btn_edit_lastname);
            btnEditDateOfBirth = view.FindViewById<ImageView>(Resource.Id.btn_edit_date_of_birth);
            btnEditHeight = view.FindViewById<ImageView>(Resource.Id.btn_edit_height);
            btnEditWeight = view.FindViewById<ImageView>(Resource.Id.btn_edit_weight);
            btnEditGender = view.FindViewById<ImageView>(Resource.Id.btn_edit_gender);

            btnEditEmail.Click += editEmail;
            btnEditPassword.Click += editPassword;
            btnEditFirstname.Click += editFirstname;
            btnEditLastname.Click += editLastname;
            btnEditDateOfBirth.Click += editDateOfBirth;
            btnEditHeight.Click += editHeight;
            btnEditWeight.Click += editWeight;
            btnEditGender.Click += editGender;

            //setup the db listener to retrieve userdata from db
            retrieveAccountData();

            return view;
        }

        /**
         *  This function fills all the textfields with the userdata that was retrieved by the db listener. 
         **/
        private void fillUserData(List<User> userdata) 
        {   
            if (userdata != null) 
            {
                titleUsername.Text = userdata[0].username;
                titleRegisteredSince.Text = Context.Resources.GetString(Resource.String.registered_since) + " " + userdata[0].registerdate;
                txtViewEmail.Text = userdata[0].email;
                txtViewPassword.Text = "********";
                txtViewFirstname.Text = userdata[0].firstname;
                txtViewLastname.Text = userdata[0].lastname;
                txtViewGender.Text = userdata[0].gender;
                txtViewDateOfBirth.Text = userdata[0].dateOfBirth;
                txtViewWeight.Text = userdata[0].weight + " kg";
                txtViewHeight.Text = userdata[0].height + " cm";
            }
        }

        /**
         *  This function initializes the db listener and the eventhandler that is triggered when new data was received from db.
         *  In this case we want to query for a dataset which has the same username as our currently logged in user that was saved
         *  inside the TemporaryData class. Since we don´t store all user info inside that class we need to query to get all attributes of the
         *  user dataset.
         **/
        private void retrieveAccountData() 
        {
            userDataListener = new FirestoreDataListener();
            userDataListener.QueryParameterized("users", "username", TemporaryData.CURRENT_USER.username);
            userDataListener.DataRetrieved += UserDataListener_UserDataRetrieved;
        }

        /**
         *  This function handles the retrieval of data from db. When data is retrieved, set it to the userList of this 
         *  fragment and then fill all the textfields with the received userdata.
         **/
        private void UserDataListener_UserDataRetrieved(object sender, FirestoreDataListener.DataEventArgs e)
        {
            userList = e.Users;
            fillUserData(userList);
        }

        /**
         *  This function handles the deletion of the user account. At first a confirmation dialog is displayed so that 
         *  the user is not automatically deleted when he pressed the delete button by accident for example. When the dialog
         *  was accepted, the deleteEntity function of the FirebaseDatalistener is called to delete the current user from db.
         *  After deletion was completed, the user is redirected to the login activity and a toast message is printed to notify 
         *  the user that the deletion was successful.
         **/
        private void deleteUserAccount(object sender, EventArgs e) 
        {
            SupportV7.AlertDialog.Builder deleteUserDialog = new SupportV7.AlertDialog.Builder(Context);
            deleteUserDialog.SetTitle(Resource.String.dialog_delete_account);
            deleteUserDialog.SetMessage(Resource.String.dialog_are_you_sure);

            deleteUserDialog.SetPositiveButton(Resource.String.dialog_accept, (senderAlert, args) =>
            {
                userDataListener.deleteEntity("users", TemporaryData.CURRENT_USER.id);
                var loginActivity = new Intent(Context, typeof(LoginActivity));
                StartActivity(loginActivity);
                Toast.MakeText(Context, Resource.String.account_deleted, ToastLength.Long).Show();
            });
            deleteUserDialog.SetNegativeButton(Resource.String.dialog_cancel, (senderAlert, args) =>
            {
                deleteUserDialog.Dispose();
            });

            deleteUserDialog.Show();
        }

        public void editEmail(object sender, EventArgs eventArgs)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View dialogView = layoutInflater.Inflate(Resource.Layout.UserInputDialog, null);

            SupportV7.AlertDialog.Builder dialogBuilder = createEditDialog(Resources.GetString(Resource.String.dialog_change_email), Resources.GetString(Resource.String.dialog_new_email), Resource.Drawable.icon_pencil, dialogView);

            var editValueField = dialogView.FindViewById<EditText>(Resource.Id.textfield_input);
            editValueField.InputType = Android.Text.InputTypes.TextVariationEmailAddress;

            dialogBuilder.SetCancelable(false)
                .SetPositiveButton(Resource.String.dialog_save, delegate
                {
                    userDataListener.updateEntity("users", TemporaryData.CURRENT_USER.id, "email", editValueField.Text);
                    Toast.MakeText(Context, Resource.String.saving_successful, ToastLength.Long).Show();
                    retrieveAccountData();
                    dialogBuilder.Dispose();
                })
                .SetNegativeButton(Resource.String.dialog_cancel, delegate
                {
                    dialogBuilder.Dispose();
                });

            SupportV7.AlertDialog dialog = dialogBuilder.Create();
            dialog.Show();
        }

        public void editPassword(object sender, EventArgs eventArgs)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View dialogView = layoutInflater.Inflate(Resource.Layout.UserPasswordInputDialog, null);

            SupportV7.AlertDialog.Builder dialogBuilder = createEditPasswordDialog(Resources.GetString(Resource.String.dialog_change_password), Resources.GetString(Resource.String.dialog_new_password), Resources.GetString(Resource.String.dialog_confirm_new_password), Resource.Drawable.icon_pencil, dialogView);
            
            var editPasswordField = dialogView.FindViewById<EditText>(Resource.Id.textview_password_input);
            var checkPasswordField = dialogView.FindViewById<EditText>(Resource.Id.textview_password_check_input);

            dialogBuilder.SetCancelable(false)
                .SetPositiveButton(Resource.String.dialog_save, delegate
                {
                    if (editPasswordField.Text == checkPasswordField.Text)
                    {
                        userDataListener.updateEntity("users", TemporaryData.CURRENT_USER.id, "password", CryptoService.Encrypt(editPasswordField.Text));
                        Toast.MakeText(Context, Resource.String.saving_successful, ToastLength.Long).Show();
                        retrieveAccountData();
                        dialogBuilder.Dispose();
                    }
                    else 
                    {
                        Toast.MakeText(Context, Resource.String.passwords_dont_match, ToastLength.Long).Show();
                    }
                })
                .SetNegativeButton(Resource.String.dialog_cancel, delegate
                {
                    dialogBuilder.Dispose();
                });

            SupportV7.AlertDialog dialog = dialogBuilder.Create();
            dialog.Show();
        }

        public void editFirstname(object sender, EventArgs eventArgs)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View dialogView = layoutInflater.Inflate(Resource.Layout.UserInputDialog, null);

            SupportV7.AlertDialog.Builder dialogBuilder = createEditDialog(Resources.GetString(Resource.String.dialog_change_first_name), Resources.GetString(Resource.String.dialog_new_first_name), Resource.Drawable.icon_pencil, dialogView);

            var editValueField = dialogView.FindViewById<EditText>(Resource.Id.textfield_input);

            dialogBuilder.SetCancelable(false)
                .SetPositiveButton(Resource.String.dialog_save, delegate
                {
                    userDataListener.updateEntity("users", TemporaryData.CURRENT_USER.id, "firstname", editValueField.Text);
                    Toast.MakeText(Context, Resource.String.saving_successful, ToastLength.Long).Show();
                    retrieveAccountData();
                    dialogBuilder.Dispose();
                })
                .SetNegativeButton(Resource.String.dialog_cancel, delegate
                {
                    dialogBuilder.Dispose();
                });

            SupportV7.AlertDialog dialog = dialogBuilder.Create();
            dialog.Show();
        }

        public void editLastname(object sender, EventArgs eventArgs)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View dialogView = layoutInflater.Inflate(Resource.Layout.UserInputDialog, null);

            SupportV7.AlertDialog.Builder dialogBuilder = createEditDialog(Resources.GetString(Resource.String.dialog_change_last_name), Resources.GetString(Resource.String.dialog_new_last_name), Resource.Drawable.icon_pencil, dialogView);

            var editValueField = dialogView.FindViewById<EditText>(Resource.Id.textfield_input);

            dialogBuilder.SetCancelable(false)
                .SetPositiveButton(Resource.String.dialog_save, delegate
                {
                    userDataListener.updateEntity("users", TemporaryData.CURRENT_USER.id, "lastname", editValueField.Text);
                    Toast.MakeText(Context, Resource.String.saving_successful, ToastLength.Long).Show();
                    retrieveAccountData();
                    dialogBuilder.Dispose();
                })
                .SetNegativeButton(Resource.String.dialog_cancel, delegate
                {
                    dialogBuilder.Dispose();
                });

            SupportV7.AlertDialog dialog = dialogBuilder.Create();
            dialog.Show();
        }

        public void editGender(object sender, EventArgs eventArgs) 
        {
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View dialogView = layoutInflater.Inflate(Resource.Layout.UserSpinnerInputDialog, null);

            SupportV7.AlertDialog.Builder dialogBuilder = createEditGenderDialog(Resources.GetString(Resource.String.dialog_change_gender), Resource.Drawable.icon_pencil, dialogView);

            var editValueSpinner = dialogView.FindViewById<Spinner>(Resource.Id.spinner_input);
            editValueSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(onSpinnerGenderItemSelected);
            var spinnerAdapter = ArrayAdapter.CreateFromResource(Context, Resource.Array.gender_array, Android.Resource.Layout.SimpleSpinnerItem);
            spinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            editValueSpinner.Adapter = spinnerAdapter;

            dialogBuilder.SetCancelable(false)
                .SetPositiveButton(Resource.String.dialog_save, delegate
                {
                    userDataListener.updateEntity("users", TemporaryData.CURRENT_USER.id, "gender", gender);
                    Toast.MakeText(Context, Resource.String.saving_successful, ToastLength.Long).Show();
                    retrieveAccountData();
                    dialogBuilder.Dispose();
                })
                .SetNegativeButton(Resource.String.dialog_cancel, delegate
                {
                    dialogBuilder.Dispose();
                });

            SupportV7.AlertDialog dialog = dialogBuilder.Create();
            dialog.Show();
        }

        public void editDateOfBirth(object sender, EventArgs eventArgs)
        {
            DatePickerFragment datePicker = DatePickerFragment.NewInstance(delegate (DateTime dateTime) 
            {
                CultureInfo cultureInfo = new CultureInfo("de-DE");
                userDataListener.updateEntity("users", TemporaryData.CURRENT_USER.id, "birthday", dateTime.ToString("d", cultureInfo));
                Toast.MakeText(Context, Resource.String.saving_successful, ToastLength.Long).Show();
                retrieveAccountData();
            });

            datePicker.Show(FragmentManager, DatePickerFragment.TAG);
        }

        public void editHeight(object sender, EventArgs eventArgs)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View dialogView = layoutInflater.Inflate(Resource.Layout.UserInputDialog, null);

            SupportV7.AlertDialog.Builder dialogBuilder = createEditDialog(Resources.GetString(Resource.String.dialog_change_height), Resources.GetString(Resource.String.dialog_new_height), Resource.Drawable.icon_pencil, dialogView);

            var editValueField = dialogView.FindViewById<EditText>(Resource.Id.textfield_input);

            dialogBuilder.SetCancelable(false)
                .SetPositiveButton(Resource.String.dialog_save, delegate
                {
                    userDataListener.updateEntity("users", TemporaryData.CURRENT_USER.id, "height", editValueField.Text);
                    Toast.MakeText(Context, Resource.String.saving_successful, ToastLength.Long).Show();
                    retrieveAccountData();
                    dialogBuilder.Dispose();
                })
                .SetNegativeButton(Resource.String.dialog_cancel, delegate
                {
                    dialogBuilder.Dispose();
                });

            SupportV7.AlertDialog dialog = dialogBuilder.Create();
            dialog.Show();
        }

        public void editWeight(object sender, EventArgs eventArgs)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View dialogView = layoutInflater.Inflate(Resource.Layout.UserInputDialog, null);

            SupportV7.AlertDialog.Builder dialogBuilder = createEditDialog(Resources.GetString(Resource.String.dialog_change_weight), Resources.GetString(Resource.String.dialog_new_weight), Resource.Drawable.icon_pencil, dialogView);

            var editValueField = dialogView.FindViewById<EditText>(Resource.Id.textfield_input);

            dialogBuilder.SetCancelable(false)
                .SetPositiveButton(Resource.String.dialog_save, delegate
                {                 
                    userDataListener.updateEntity("users", TemporaryData.CURRENT_USER.id, "weight", editValueField.Text);
                    Toast.MakeText(Context, Resource.String.saving_successful, ToastLength.Long).Show();
                    retrieveAccountData();
                    dialogBuilder.Dispose();
                })
                .SetNegativeButton(Resource.String.dialog_cancel, delegate
                {
                    dialogBuilder.Dispose();
                });

            SupportV7.AlertDialog dialog = dialogBuilder.Create();
            dialog.Show();
        }

        public SupportV7.AlertDialog.Builder createEditDialog(string title, string placeholder, int iconId, View parentView) 
        {
            SupportV7.AlertDialog.Builder dialogBuilder = new SupportV7.AlertDialog.Builder(Context);
            dialogBuilder.SetView(parentView);

            dialogBuilder.SetTitle(title);
            dialogBuilder.SetIcon(iconId);

            var editValueField = parentView.FindViewById<EditText>(Resource.Id.textfield_input);
            editValueField.Hint = placeholder;

            return dialogBuilder;
        }

        public SupportV7.AlertDialog.Builder createEditPasswordDialog(string title, string placeholderPassword, string placeholderPasswordCheck, int iconId, View parentView)
        {
            SupportV7.AlertDialog.Builder dialogBuilder = new SupportV7.AlertDialog.Builder(Context);
            dialogBuilder.SetView(parentView);

            dialogBuilder.SetTitle(title);
            dialogBuilder.SetIcon(iconId);

            var editPasswordField = parentView.FindViewById<EditText>(Resource.Id.textview_password_input);
            editPasswordField.Hint = placeholderPassword;

            var checkPasswordField = parentView.FindViewById<EditText>(Resource.Id.textview_password_check_input);
            checkPasswordField.Hint = placeholderPasswordCheck;

            return dialogBuilder;
        }

        public SupportV7.AlertDialog.Builder createEditGenderDialog(string title, int iconId, View parentView)
        {
            SupportV7.AlertDialog.Builder dialogBuilder = new SupportV7.AlertDialog.Builder(Context);
            dialogBuilder.SetView(parentView);

            dialogBuilder.SetTitle(title);
            dialogBuilder.SetIcon(iconId);

            return dialogBuilder;
        }

        private void onSpinnerGenderItemSelected(object sender, AdapterView.ItemSelectedEventArgs e) 
        {
            Spinner spinner = (Spinner)sender;
            gender = spinner.GetItemAtPosition(e.Position).ToString();
        }
    }
}
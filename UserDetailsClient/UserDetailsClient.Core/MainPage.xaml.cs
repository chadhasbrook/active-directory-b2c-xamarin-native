﻿using System;
using System.Net.Http;
using Microsoft.Identity.Client;
using UserDetailsClient.Core.Features.LogOn;
using Xamarin.Forms;

namespace UserDetailsClient.Core
{
    public partial class MainPage : ContentPage
    {
        protected readonly IAuthenticationService authenticationService;

        public MainPage()
        {
            InitializeComponent();

            /* Grab an instance of the IAuthenticationService using DependencyService.
             * 
             * NOTE: this will give us an instance of B2CAuthenticationService 
             * because we registered that class in App.xaml.cs
             * 
             * */
            authenticationService = DependencyService.Get<IAuthenticationService>();
        }

        async void OnSignInSignOut(object sender, EventArgs e)
        {
            try
            {
                if (btnSignInSignOut.Text == "Sign in")
                {
                    var userContext = await authenticationService.SignInAsync();
                    UpdateSignInState(userContext);
                    UpdateUserInfo(userContext);
                }
                else
                {
                    var userContext = await authenticationService.SignOutAsync();
                    UpdateSignInState(userContext);
                    UpdateUserInfo(userContext);
                }
            }
            catch (Exception ex)
            {
                // Checking the exception message 
                // should ONLY be done for B2C
                // reset and not any other error.
                if (ex.Message.Contains("AADB2C90118"))
                    OnPasswordReset();
                // Alert if any exception excluding user cancelling sign-in dialog
                else if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }
        async void OnCallApi(object sender, EventArgs e)
        {
            try
            {
                lblApi.Text = $"Calling API {App.ApiEndpoint}";
                var userContext = await authenticationService.SignInAsync();
                var token = userContext.AccessToken;

                // Get data from API
                HttpClient client = new HttpClient();
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, App.ApiEndpoint);
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(message);
                string responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    lblApi.Text = $"Response from API {App.ApiEndpoint} | {responseString}";
                }
                else
                {
                    lblApi.Text = $"Error calling API {App.ApiEndpoint} | {responseString}";
                }
            }
            catch (MsalUiRequiredException ex)
            {
                await DisplayAlert($"Session has expired, please sign out and back in.", ex.ToString(), "Dismiss");
            }
            catch (Exception ex)
            {
                await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        async void OnEditProfile(object sender, EventArgs e)
        {
            try
            {
                var userContext = await authenticationService.EditProfileAsync();
                UpdateSignInState(userContext);
                UpdateUserInfo(userContext);
            }
            catch (Exception ex)
            {
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }
        async void OnResetPassword(object sender, EventArgs e)
        {
            try
            {
                var userContext = await authenticationService.ResetPasswordAsync();
                UpdateSignInState(userContext);
                UpdateUserInfo(userContext);
            }
            catch (Exception ex)
            {
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }
        async void OnPasswordReset()
        {
            try
            {
                var userContext = await authenticationService.ResetPasswordAsync();
                UpdateSignInState(userContext);
                UpdateUserInfo(userContext);
            }
            catch (Exception ex)
            {
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        void UpdateSignInState(UserContext userContext)
        {
            var isSignedIn = userContext.IsLoggedOn;
            btnSignInSignOut.Text = isSignedIn ? "Sign out" : "Sign in";
            btnEditProfile.IsVisible = isSignedIn;
            btnCallApi.IsVisible = isSignedIn;
            slUser.IsVisible = isSignedIn;
            lblApi.Text = "";
        }
        public void UpdateUserInfo(UserContext userContext)
        {
            lblName.Text = userContext.Name;
            lblJob.Text = userContext.JobTitle;
            lblCity.Text = userContext.City;
        }
    }
}
/*
 *  Copyright (C)  2020 Kynetics, LLC
 *  SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Kynetics.Ethernetmanager;
using Com.Kynetics.Ethernetmanager.Model;
using Java.Net;

namespace ExampleApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        const String TAG = " MainActivity";
        EditText ipaddress;
        EditText gateway;
        EditText dns;
        EditText name;
        RadioButton radio_get;
        RadioButton radio_set;
        RadioButton radio_static;
        RadioButton radio_dhcp;
        Button set_button;
        Button get_button;
        SystemEthernetManager eth;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            eth = SystemEthernetManager.Get(this);


            radio_get = FindViewById<RadioButton>(Resource.Id.radio_get);
            radio_set = FindViewById<RadioButton>(Resource.Id.radio_set);
            radio_static = FindViewById<RadioButton>(Resource.Id.radio_static);
            radio_dhcp = FindViewById<RadioButton>(Resource.Id.radio_dhcp);

            set_button = FindViewById<Button>(Resource.Id.set_button);
            get_button = FindViewById<Button>(Resource.Id.get_button);

            ipaddress = FindViewById<EditText>(Resource.Id.ip_address);
            gateway = FindViewById<EditText>(Resource.Id.gateway);
            dns = FindViewById<EditText>(Resource.Id.dns);
            name = FindViewById<EditText>(Resource.Id.interface_name);

            radio_get.Click += RadioSetGetButtonClick;
            radio_set.Click += RadioSetGetButtonClick;
            radio_static.Click += RadioStaticDHCPButtonClick;
            radio_dhcp.Click += RadioStaticDHCPButtonClick;

            get_button.Click += GetButtonClick;
            set_button.Click += SetButtonClick;

            get_button.Visibility = ViewStates.Gone;
            set_button.Visibility = ViewStates.Gone;



        }

        private void GetButtonClick(object sender, EventArgs e)
        {
            IpConfiguration ipConfiguration = GetConf();
            if(ipConfiguration == null)
            {
                return;
            }

            if (ipConfiguration.GetIpAssignment() == IpConfiguration.IpAssignment.Dhcp)
            {
                radio_dhcp.Checked = true;
            }
            else if(ipConfiguration.StaticIpConfiguration != null)
            {
                radio_static.Checked = true;
                if (ipConfiguration.StaticIpConfiguration.IpAddress != null) {
                    ipaddress.Text = ipConfiguration.StaticIpConfiguration.IpAddress.ToString();
                } else 
                gateway.Text = ipConfiguration.StaticIpConfiguration.Gateway.HostAddress;
                if (ipConfiguration.StaticIpConfiguration.DnsServers.Count > 0)
                {
                    dns.Text = ipConfiguration.StaticIpConfiguration.DnsServers[0].HostAddress;
                }
            } else
            {
                Toast.MakeText(this, "Interface not configured", ToastLength.Long).Show();
            }

        }

        private IpConfiguration GetConf()
        {
            try
            {
                return eth.GetConfiguration(name.Text);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, String.Format("Interface named {0} not found", name.Text), ToastLength.Long).Show();
                Log.Warn(TAG, "GetConf error", ex);
                return null;
            }
        }
        private void SetButtonClick(object sender, EventArgs e)
        {
            IpConfiguration ipConfiguration = GetConf();
            if (ipConfiguration == null)
            {
                return;
            }

            if (radio_dhcp.Checked)
            {
                SetConf(new IpConfiguration(IpConfiguration.IpAssignment.Dhcp, null, IpConfiguration.ProxySettings.Unassigned, null));
            }
            else if (radio_static.Checked)
            {
                List<InetAddress> dnsList = new List<InetAddress>();
                dnsList.Add(InetAddress.GetByName(dns.Text));
                IpConfiguration ipConf = new IpConfiguration(
                    IpConfiguration.IpAssignment.Static,
                    new StaticIpConfiguration(ipaddress.Text, InetAddress.GetByName(gateway.Text), dnsList, null),
                    IpConfiguration.ProxySettings.Unassigned,
                    null) ;

                SetConf(ipConf);
            }
        }


        private void SetConf(IpConfiguration newConf)
        {
            try
            {
                eth.SetConfiguration(name.Text, newConf);
            } catch (Exception e)
            {
                Toast.MakeText(this, "Error on ethernet configuration", ToastLength.Long).Show();
                Log.Warn(TAG, "SetConf error", e);
            }

        }
        private void RadioSetGetButtonClick(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            Boolean enabled = rb.Text == "Set Conf";

            ipaddress.Text = "";
            ipaddress.Enabled = enabled;

            gateway.Text = "";
            gateway.Enabled = enabled;

            dns.Text = "";
            dns.Enabled = enabled;

            radio_static.Enabled = enabled;
            radio_dhcp.Enabled = enabled;

            get_button.Visibility = enabled ? ViewStates.Gone : ViewStates.Visible;
            set_button.Visibility = enabled ? ViewStates.Visible : ViewStates.Gone;
        }

        private void RadioStaticDHCPButtonClick(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            Boolean enabled = rb.Text == "Static";
            ipaddress.Text = "";
            ipaddress.Enabled = enabled;

            gateway.Text = "";
            gateway.Enabled = enabled;

            dns.Text = "";
            dns.Enabled = enabled;

            radio_static.Enabled = true;
            radio_dhcp.Enabled = true;

        }



        private void RadioButtonClick(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            Toast.MakeText(this, rb.Text, ToastLength.Short).Show();
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}


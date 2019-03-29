using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AlertDialog = Android.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace _7chafen_sharp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private string token { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var toolbar =
                FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            Login();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;
            if (id == Resource.Id.action_about)
            {
                var alert = new AlertDialog.Builder(this);
                alert.SetTitle("关于");
                alert.SetMessage("七天网络第三方查分 App.");
                alert.SetPositiveButton("确定", (senderAlert, args) => { });

                alert.SetNeutralButton("Github",
                    (senderAlert, args) => { Toast.MakeText(this, "已开源在 Github!", ToastLength.Short).Show(); });


                Dialog dialog = alert.Create();
                dialog.Show();
                return true;
            }

            if (id == Resource.Id.action_logout)
            {
                GetSharedPreferences("config", FileCreationMode.Private)
                    .Edit().PutString("token", "").Apply();
                ShowLoginDialog();
            }

            return base.OnOptionsItemSelected(item);
        }

        private async void Login()
        {
            var storedToken = GetSharedPreferences("config", FileCreationMode.Private).GetString("token", "");
            if (await API.Validate(storedToken))
            {
                token = storedToken;
                refresh();
            }
            else
            {
                ShowLoginDialog();
            }
        }

        private void ShowLoginDialog()
        {
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("请登录");
            var view = View.Inflate(this, Resource.Layout.login_dialog, null);
            alert.SetView(view);
            alert.SetPositiveButton("登录", (EventHandler<DialogClickEventArgs>) null);
            var dialog = alert.Show();
            dialog.GetButton((int) DialogButtonType.Positive).Click += async (sender, args) =>
            {
                var username = view.FindViewById<EditText>(Resource.Id.et_name).Text;
                var password = view.FindViewById<EditText>(Resource.Id.et_pwd).Text;
                var token = await API.Login(username, password);
                if (token != null)
                {
                    Toast.MakeText(this, "登录成功！", ToastLength.Short).Show();
                    refresh();
                    this.token = token;
                    GetSharedPreferences("config", FileCreationMode.Private)
                        .Edit().PutString("token", token).Apply();
                    dialog.Dismiss();
                }

                Toast.MakeText(this, "登录失败！", ToastLength.Short).Show();
            };
        }

        private async void refresh()
        {
            var exams = await API.GetExams(token);
            var listView = FindViewById<ListView>(Resource.Id.listView1);
            var nameList = exams.Select(x => x.name).ToList();
            listView.Adapter = new ArrayAdapter<string>(this, Resource.Layout.list_item, nameList);
            listView.ItemClick += (sender, args) =>
            {
                Toast.MakeText(this, exams[args.Position].name, ToastLength.Short).Show();
            };
        }
    }
}
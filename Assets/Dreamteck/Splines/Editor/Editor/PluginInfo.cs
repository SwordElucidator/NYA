using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace Dreamteck.Splines
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class PluginInfo
    {
        public static string version = "1.0.6";
        private static bool open = false;
        static PluginInfo()
        {
            if (open) return;
            bool showInfo = EditorPrefs.GetBool("ShowPluginInfo" + version, true);
            if (!showInfo) return;
            InitWindow window = EditorWindow.GetWindow<InitWindow>(true);
            window.init();
            EditorPrefs.SetBool("ShowPluginInfo" + PluginInfo.version, false);
            open = true;
        }
    }

    public class InitWindow : EditorWindow
    {
        public class WindowPanel
        {
            public float slideStart = 0f;
            public float slideDuration = 1f;
            public enum SlideDiretion { Left, Right, Up, Down }
            public SlideDiretion slideDirection = SlideDiretion.Left;
            public Vector2 size;
            private Vector2 origin = Vector2.zero;
            public bool open = false;

            public bool isActive
            {
                get
                {
                    return open || Time.realtimeSinceStartup - slideStart <= slideDuration;
                }
            }

            void HandleOrigin()
            {
                float percent = Mathf.Clamp01((Time.realtimeSinceStartup - slideStart) / slideDuration);
                if (open)
                {
                    switch (slideDirection)
                    {
                        case SlideDiretion.Left:
                            origin.x = Mathf.SmoothStep(size.x, 0f, percent);
                            origin.y = 0f;
                            break;

                        case SlideDiretion.Right:
                            origin.x = Mathf.SmoothStep(-size.x, 0f, percent);
                            origin.y = 0f;
                            break;

                        case SlideDiretion.Up:
                            origin.x = 0f;
                            origin.y = Mathf.SmoothStep(size.y, 0f, percent);
                            break;

                        case SlideDiretion.Down:
                            origin.x = 0f;
                            origin.y = Mathf.SmoothStep(-size.y, 0f, percent);
                            break;
                    }
                }
                else
                {
                    switch (slideDirection)
                    {
                        case SlideDiretion.Left:
                            origin.x = Mathf.SmoothStep(0f, -size.x, percent);
                            origin.y = 0f;
                            break;

                        case SlideDiretion.Right:
                            origin.x = Mathf.SmoothStep(0f, size.x, percent);
                            origin.y = 0f;
                            break;

                        case SlideDiretion.Up:
                            origin.x = 0f;
                            origin.y = Mathf.SmoothStep(0f, -size.y, percent);
                            break;

                        case SlideDiretion.Down:
                            origin.x = 0f;
                            origin.y = Mathf.SmoothStep(0f, -size.y, percent);
                            break;
                    }
                }
            }

            public void SetState(bool state, bool useTransition)
            {
                if (open == state) return;
                open = state;
                if (useTransition) slideStart = Time.realtimeSinceStartup;
                else slideStart = Time.realtimeSinceStartup + slideDuration;
            }

            public void Begin()
            {
                HandleOrigin();
                GUILayout.BeginArea(new Rect(origin.x, origin.y, size.x, size.y));
            }

            public void BackButton(WindowPanel backButtonPanel, SlideDiretion dir = SlideDiretion.Left)
            {
                if (GUILayout.Button("◄", GUILayout.Width(60), GUILayout.Height(35)))
                {
                    slideDirection = dir;
                    SetState(false, true);
                    backButtonPanel.slideDirection = dir;
                    backButtonPanel.SetState(true, true);
                }
            }

            public void OpenPanel(WindowPanel backButtonPanel, SlideDiretion dir = SlideDiretion.Left)
            {
                slideDirection = dir;
                SetState(false, true);
                backButtonPanel.slideDirection = dir;
                backButtonPanel.SetState(true, true);
            }

            public void End()
            {
                GUILayout.EndArea();
            }
        }

        private GUIStyle titleText;
        private GUIStyle buttonTitleText;
        private GUIStyle warningText;
        private GUIStyle wrapText;
        private bool initialized = false;

        WindowPanel changeLogPanel;
        WindowPanel homePanel;
        WindowPanel supportPanel;
        WindowPanel learnPanel;

        string mailError = "";

        private bool changeLog = true;
        private bool learn = false;
        private bool support = false;

        string email = "";
        string subject = "";
        string message = "";
        string senderName = "";
        bool mailSent = false;

        private string changelogText = "changelog.txt wasn't found. No Changelog information available.";

        Texture2D header;

        Texture2D changelogIcon;
        Texture2D supportIcon;
        Texture2D learnIcon;
        Texture2D rateIcon;
        Texture2D videoIcon;
        Texture2D pdfIcon;

        private Vector2 scroll;

        [MenuItem("Window/Dreamteck/Splines/Plugin Info")]
        public static void OpenWindow()
        {
            InitWindow window = EditorWindow.GetWindow<InitWindow>(true);
            window.init(); 
        }

        public void init()
        {
            minSize = maxSize = new Vector2(450, 500);
#if UNITY_5_0
                title = "Dreamteck Splines " + PluginInfo.version;
#else
            titleContent = new GUIContent("Dreamteck Splines " + PluginInfo.version);
#endif
            position = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 200, 450, 500);

            changeLogPanel = new WindowPanel();
            supportPanel = new WindowPanel();
            homePanel = new WindowPanel();
            learnPanel = new WindowPanel();
            changeLogPanel.size = supportPanel.size = homePanel.size = learnPanel.size = new Vector2(maxSize.x, maxSize.y - 82);
            changeLogPanel.slideDuration = supportPanel.slideDuration = homePanel.slideDuration = learnPanel.slideDuration = 0.25f;
            homePanel.SetState(true, false);
            header = SplineEditorGUI.LoadTexture("plugin_header.png");
            changelogIcon = SplineEditorGUI.LoadTexture("changelog.png");
            learnIcon = SplineEditorGUI.LoadTexture("get_started.png");
            supportIcon = SplineEditorGUI.LoadTexture("support.png");
            rateIcon = SplineEditorGUI.LoadTexture("rate.png");
            pdfIcon = SplineEditorGUI.LoadTexture("pdf.png");
            videoIcon = SplineEditorGUI.LoadTexture("video_tutorials.png");

            string path = Application.dataPath + "/Dreamteck/Splines/Editor/";
            if (Directory.Exists(path))
            {
                if (File.Exists(path + "changelog.txt")){
                    string[] lines = File.ReadAllLines(path + "changelog.txt");
                    changelogText = "";
                    for(int i = 0; i < lines.Length; i++)
                    {
                        changelogText += lines[i] + "\r\n";
                    }
                }
            }
        }

        void OnGUI()
        {
            if (!initialized)
            {
                initialized = true;
                buttonTitleText = new GUIStyle(GUI.skin.GetStyle("label"));
                buttonTitleText.fontStyle = FontStyle.Bold;
                titleText = new GUIStyle(GUI.skin.GetStyle("label"));
                titleText.fontSize = 25;
                titleText.fontStyle = FontStyle.Bold;
                titleText.alignment = TextAnchor.MiddleLeft;
                titleText.normal.textColor = Color.white;
                warningText = new GUIStyle(GUI.skin.GetStyle("label"));
                warningText.fontSize = 18;
                warningText.fontStyle = FontStyle.Bold;
                warningText.normal.textColor = Color.red;
                warningText.alignment = TextAnchor.MiddleCenter;
                wrapText = new GUIStyle(GUI.skin.GetStyle("label"));
                wrapText.wordWrap = true;
            }

            GUI.DrawTexture(new Rect(0, 0, maxSize.x, 82), header, ScaleMode.StretchToFill);
            GUI.Label(new Rect(126, 15, 115, 50), PluginInfo.version, titleText);
            GUILayout.BeginArea(new Rect(0, 85, maxSize.x, maxSize.y - 85));
            Home();
            ChangeLog();
            Support();
            Learn();
            GUILayout.EndArea();
        }

        void Home()
        {
            if (!homePanel.isActive) return;
            homePanel.Begin();
            if(MenuButton(new Vector2(25, 25), changelogIcon, "Changelog", "See what's new in version " + PluginInfo.version))
            {
                homePanel.OpenPanel(changeLogPanel, WindowPanel.SlideDiretion.Left);
            }

            if (MenuButton(new Vector2(25, 85), learnIcon, "Get Started", "Learn how to use Dreamteck Splines in a matter of minutes."))
            {
                homePanel.OpenPanel(learnPanel, WindowPanel.SlideDiretion.Left);
            }

            if (MenuButton(new Vector2(25, 145), supportIcon, "Support", "Got a problem or a feature request? Our support is here to help!"))
            {
                homePanel.OpenPanel(supportPanel, WindowPanel.SlideDiretion.Left);
            }

            if (MenuButton(new Vector2(25, 240), rateIcon, "Rate", "If you like Dreamteck Splines, please consider rating it on the Asset Store"))
            {
                Application.OpenURL("http://u3d.as/sLk");
            }


            GUI.Label(new Rect(25, 320, 400, 80), "This window will not show automatically again. If you need it, you can always open it by going to Window->Dreamteck->Splines->Plugin Info", wrapText);

            if (GUI.Button(new Rect(350, 360, 70, 35), "Close")) Close();
            Repaint();
            homePanel.End();
        }

        void Support()
        {
            if (!supportPanel.isActive) return;
            supportPanel.Begin();
            supportPanel.BackButton(homePanel, WindowPanel.SlideDiretion.Right);
            GUILayout.BeginArea(new Rect(25, 65, 400, 300));
            if (mailSent)
            {
                GUILayout.Label("Message sent. A contact web page should have opened notifying that the message has been sent. If that hasn't happened, please write a message directly to support@dreamteck-hq.com.", wrapText);
                if (GUILayout.Button("New Message")) mailSent = false;
            }
            else if (mailError != "")
            {
                GUILayout.Label(mailError);
                if (GUILayout.Button("OK")) mailError = "";
            }
            else
            {
                email = EditorGUILayout.TextField("Your E-mail:", email);
                senderName = EditorGUILayout.TextField("Your name:", senderName);
                subject = EditorGUILayout.TextField("Subject:", subject);
                GUILayout.Label("Message:");
                message = GUILayout.TextArea(message, GUILayout.MinHeight(60));
                GUILayout.Label("The message is limited to 200 characters. If you want to send a longer message, contact us directly. Left: " + (200-message.Length), wrapText);
                if (message.Length > 200) message = message.Substring(0, 200);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Send", GUILayout.Height(35), GUILayout.Width(50)))
                {
                    SendMail();
                }
                GUILayout.Label("Sending to:");
                GUILayout.TextField("support@dreamteck-hq.com", GUILayout.Height(25));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Dreamteck Website", GUILayout.Height(35))) Application.OpenURL("http://dreamteck-hq.com");
            GUILayout.EndArea();

            Repaint();
            supportPanel.End();
        }

        void SendMail()
        {
            if (subject.Length <= 2) mailError = "ERROR: Subject is too short, please enter a valid subject";
            else if (message.Length <= 10) mailError = "ERROR: Message is too short. Please enter a valid message.";
            else if (Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"))
            {
                string url = "http://dreamteck-hq.com/team/contact.php?support_message=1&n=" + WWW.EscapeURL(senderName) + "&s=" + WWW.EscapeURL(subject) + "&e=" + WWW.EscapeURL(email) + "&m=" + WWW.EscapeURL(message);
                Application.OpenURL(url);
                mailError = "";
                message = subject = "";
                mailSent = true;
            } else mailError = "ERROR: Invalid e-mail address. Plase provide your e-mail address so we can contact you.";
           
        }

        void Learn()
        {
            if (!learnPanel.isActive) return;
            learnPanel.Begin();
            learnPanel.BackButton(homePanel, WindowPanel.SlideDiretion.Right);

            if (MenuButton(new Vector2(25, 40), videoIcon, "Video Tutorials", "Watch a series of Youtube videos to get started."))
            {
                Application.OpenURL("https://www.youtube.com/playlist?list=PLkZqalQdFIQ4S-UGPWCZTTZXiE5MebrVo");
            }

            if (MenuButton(new Vector2(25, 100), pdfIcon, "User Manual", "Read a thorough documentation of the whole package."))
            {
                Application.OpenURL("http://dreamteck-hq.com/page/dreamteck_splines/user_manual.pdf");
            }

            if (MenuButton(new Vector2(25, 160), pdfIcon, "API Reference", "A documentation of the programmers' part of the package."))
            {
                Application.OpenURL("http://dreamteck-hq.com/page/dreamteck_splines/api_reference.pdf");
            }
            Repaint();
            learnPanel.End();
        }

        void ChangeLog()
        {
            if (!changeLogPanel.isActive) return;
            changeLogPanel.Begin();
            changeLogPanel.BackButton(homePanel, WindowPanel.SlideDiretion.Right);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(maxSize.x), GUILayout.MaxHeight(400));
            //GUILayout.Label("VERSION " + PluginInfo.version + " WARNING", warningText, GUILayout.Width(maxSize.x - 30), GUILayout.Height(30));
            string text = " In version 1.0.6 the whole editor has been re-written in order to improve performance and add flexibility for future expansions. Two major features have been added - the Symmetry editor and the Spline Merge editor. More can be read about them in the user manual.";
            EditorGUILayout.LabelField(text, wrapText, GUILayout.Width(maxSize.x - 30));
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(changelogText, wrapText, GUILayout.Width(maxSize.x - 30));
            GUILayout.EndScrollView();
            Repaint();
            changeLogPanel.End();
        }

        bool MenuButton(Vector2 position, Texture2D image, string title, string description)
        {
            bool result = false;
            Rect rect = new Rect(position.x, position.y, 400, 50);
            Color buttonColor = Color.clear;
            if (rect.Contains(Event.current.mousePosition)) buttonColor = Color.white;
            GUI.BeginGroup(rect);
            GUI.color = buttonColor;
            if (GUI.Button(new Rect(0, 0, 400, 50), "")) result = true;
            GUI.color = Color.white;
            if(image != null) GUI.DrawTexture(new Rect(0, 0, 50, 50), image, ScaleMode.StretchToFill);
            GUI.Label(new Rect(60, 5, 370 - 65, 16), title, buttonTitleText);
            GUI.Label(new Rect(60, 20, 370 - 65, 40), description, wrapText);
            GUI.EndGroup();
            return result;
        }
    }
#endif
}

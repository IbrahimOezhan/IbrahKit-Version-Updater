using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace IbrahKit
{
    public class VersionUpdater : EditorWindow
    {
        string currentVersion;

        const string regex = @"^(\D*)(\d+(?:\.\d+)*)(\D+)?$";

        [MenuItem("IbrahKit/VersionUpdater")]
        public static void ShowExample()
        {
            VersionUpdater updater = GetWindow<VersionUpdater>();
            updater.titleContent = new GUIContent("VersionUpdater");
        }

        private void CreateGUI()
        {
            currentVersion = string.Empty;

            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 25;
            root.style.paddingBottom = 25;
            root.style.paddingRight = 25;
            root.style.paddingTop = 25;
            root.style.flexDirection = FlexDirection.Column;
        }

        private void OnEnable()
        {
            if (NeedsReload()) UpdateWindow();
        }

        private void OnBecameVisible()
        {
            if (NeedsReload()) UpdateWindow();
        }

        private void OnFocus()
        {
            if (NeedsReload()) UpdateWindow();
        }

        private void UpdateWindow()
        {
            currentVersion = Application.version;

            VisualElement root = rootVisualElement;

            root.Clear();

            Label title = new("Version Updater");
            title.style.fontSize = 30;
            root.Add(title);

            AddLine(root);

            Match match = Regex.Match(currentVersion, regex);

            if (match.Success)
            {
                string[] splitNumbers = match.Groups[2].Value.Split(".");

                Button applyButton = new();
                root.Add(applyButton);

                AddLine(root);

                VisualElement versionRoot = new();
                root.Add(versionRoot);

                Button[] ups = new Button[splitNumbers.Length];

                IntegerField[] fields = new IntegerField[splitNumbers.Length];

                Button[] downs = new Button[splitNumbers.Length];

                VisualElement[] columns = new VisualElement[splitNumbers.Length];

                Label currVersion = new();
                root.Add(currVersion);

                Label newVersion = new();
                root.Add(newVersion);

                AddLine(root);

                Button resetButton = new();
                root.Add(resetButton);

                for (int i = 0; i < columns.Length; i++)
                {
                    VisualElement column = new();
                    columns[i] = column;
                    versionRoot.Add(column);

                    Button up = new();
                    ups[i] = up;
                    column.Add(up);

                    IntegerField field = new();
                    fields[i] = field;
                    column.Add(field);

                    Button down = new();
                    downs[i] = down;
                    column.Add(down);
                }

                applyButton.style.height = 50;
                applyButton.style.fontSize = 20;

                currVersion.style.fontSize = 20;
                newVersion.style.fontSize = 20;

                resetButton.style.fontSize = 20;
                resetButton.text = "Reset";

                versionRoot.style.flexDirection = FlexDirection.Row;
                versionRoot.style.flexGrow = 1;

                for (int i = 0; i < splitNumbers.Length; i++)
                {
                    columns[i].style.flexDirection = FlexDirection.Column;

                    ups[i].text = "+";
                    ups[i].style.fontSize = 30;

                    fields[i].value = int.Parse(splitNumbers[i]);
                    fields[i].style.fontSize = 30;

                    downs[i].text = "-";
                    downs[i].style.fontSize = 30;
                    downs[i].pickingMode = fields[i].value > 0 ? PickingMode.Position : PickingMode.Ignore;
                }

                for (int i = 0; i < splitNumbers.Length; i++)
                {
                    int j = i;

                    int max = splitNumbers.Length;

                    ups[i].clicked += () =>
                    {
                        fields[j].value++;

                        for (int u = j + 1; u < max; u++)
                        {
                            fields[u].value = 0;
                        }

                        downs[j].pickingMode = fields[j].value > 0 ? PickingMode.Position : PickingMode.Ignore;

                        UpdateUI(currVersion,newVersion,applyButton, fields, match);
                    };

                    downs[i].clicked += () =>
                    {
                        fields[j].value--;

                        for (int u = j + 1; u < max; u++)
                        {
                            fields[u].value = 0;
                        }

                        downs[j].pickingMode = fields[j].value > 0 ? PickingMode.Position : PickingMode.Ignore;

                        UpdateUI(currVersion,newVersion,applyButton, fields, match);
                    };
                }

                applyButton.clicked += () =>
                {
                    currentVersion = GetFinalVersion(fields, match);

                    PlayerSettings.bundleVersion = currentVersion;
                    
                    UpdateUI(currVersion,newVersion,applyButton, fields, PlayerSettings.bundleVersion);
                };


                resetButton.clicked += () => UpdateWindow();
                
                UpdateUI(currVersion,newVersion, applyButton, fields, match);
            }
            else
            {
                Label label = new($"{Application.version} does not match regex {regex}");
                
                label.style.fontSize = 20;
                
                label.style.alignContent = Align.Center;
                
                root.Add(label);
            }
        }

        private void UpdateUI(Label currVersion, Label newVersion,Button button, IntegerField[] fields, Match match)
        {
            string final = GetFinalVersion(fields, match);

            UpdateUI(currVersion,newVersion, button, fields, final);
        }

        private void UpdateUI(Label currVersion,Label newVersion, Button button, IntegerField[] fields, string final)
        {
            newVersion.text = "Version in VersionUpdater: " + final;

            currVersion.text = "Version in PlayerSettings: " + currentVersion;

            bool enabled = !currentVersion.Equals(final);

            button.pickingMode = enabled ? PickingMode.Position : PickingMode.Ignore;
            
            button.text = enabled ? "Apply version to PlayerSettings" : $"Version {final} already in PlayerSettings";

            button.style.backgroundColor = enabled ? new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.5f)) : new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.25f));
        }

        private string GetFinalVersion(IntegerField[] fields, Match match)
        {
            string s = string.Empty;

            for (int i = 0; i < fields.Length; i++)
            {
                s += fields[i].value + (i < fields.Length - 1 ? "." : "");
            }

            s = match.Groups[1] + s;

            return s;
        }

        private void AddLine(VisualElement parent)
        {
            VisualElement line = new();
            
            line.style.height = 1;
            
            line.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            
            line.style.marginTop = 4;
            
            line.style.marginBottom = 4;
            
            parent.Add(line);
        }

        private bool NeedsReload()
        {
            return !Application.version.Equals(currentVersion);
        }
    }
}
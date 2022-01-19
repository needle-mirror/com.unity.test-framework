using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.GUI.Controls;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    [Serializable]
    internal class TestRunnerUIFilter
    {
        const AssemblyType k_DefaultAssemblyTypeSelection = AssemblyType.EditorOnly | AssemblyType.EditorAndPlatforms;
        const RequirePlaymodeMode k_DefaultRequirePlaymodeModeSelection =
            RequirePlaymodeMode.TestsRequiringPlaymodeInEditor | RequirePlaymodeMode.TestsNotRequiringPlaymodeInEditor;

        private int m_PassedCount;
        private int m_FailedCount;
        private int m_NotRunCount;
        private int m_InconclusiveCount;
        private int m_SkippedCount;

        public int PassedCount { get { return m_PassedCount; } }
        public int FailedCount { get { return m_FailedCount + m_InconclusiveCount; } }
        public int NotRunCount { get { return m_NotRunCount + m_SkippedCount; } }

        [SerializeField]
        public bool PassedHidden;
        [SerializeField]
        public bool FailedHidden;
        [SerializeField]
        public bool NotRunHidden;

        [field: SerializeField]
        public TestPlatformSelection SelectedTestPlatform { get; private set; }

        [field: SerializeField]
        public RequirePlaymodeMode SelectedTestMode { get; private set; } =
            k_DefaultRequirePlaymodeModeSelection;
        [field: SerializeField]
        public AssemblyType SelectedAssemblyTypes { get; private set; } = k_DefaultAssemblyTypeSelection;
        [SerializeField]
        public string m_SearchString;
        [SerializeField]
        private string[] selectedCategories = new string[0];

        public string[] availableCategories = new string[0];


        private GUIContent m_SucceededBtn;
        private GUIContent m_FailedBtn;
        private GUIContent m_NotRunBtn;

        public Action RebuildTestList;
        public Action UpdateTestTreeRoots;
        public Action<string> SearchStringChanged;
        public Action SearchStringCleared;
        public Action BuildPlayer;
        public bool IsFiltering
        {
            get
            {
                return !string.IsNullOrEmpty(m_SearchString) || PassedHidden || FailedHidden || NotRunHidden ||
                    (selectedCategories != null && selectedCategories.Length > 0) || IsFilteringTestMode();
            }
        }

        bool IsFilteringTestMode()
        {
            return SelectedTestMode != k_DefaultRequirePlaymodeModeSelection
                || IsAssemblyTypeFilteringPossible() && SelectedAssemblyTypes != k_DefaultAssemblyTypeSelection;
        }

        bool IsAssemblyTypeFilteringPossible()
        {
            var testPlatformTarget = SelectedTestPlatform.PlatformTarget;
            return testPlatformTarget == TestPlatformTarget.Editor || testPlatformTarget == TestPlatformTarget.CustomRunner;
        }

        public string[] CategoryFilter
        {
            get { return selectedCategories; }
        }

        public void UpdateCounters(List<TestRunnerResult> resultList, Dictionary<string, TestTreeViewItem> filteredTree)
        {
            m_PassedCount = m_FailedCount = m_NotRunCount =  m_InconclusiveCount = m_SkippedCount = 0;
            foreach (var result in resultList)
            {
                if (result.isSuite)
                    continue;
                if (filteredTree != null && !filteredTree.ContainsKey(result.fullName))
                    continue;
                switch (result.resultStatus)
                {
                    case TestRunnerResult.ResultStatus.Passed:
                        m_PassedCount++;
                        break;
                    case TestRunnerResult.ResultStatus.Failed:
                        m_FailedCount++;
                        break;
                    case TestRunnerResult.ResultStatus.Inconclusive:
                        m_InconclusiveCount++;
                        break;
                    case TestRunnerResult.ResultStatus.Skipped:
                        m_SkippedCount++;
                        break;
                    case TestRunnerResult.ResultStatus.NotRun:
                    default:
                        m_NotRunCount++;
                        break;
                }
            }

            var succeededTooltip = string.Format("Show tests that succeeded\n{0} succeeded", m_PassedCount);
            m_SucceededBtn = new GUIContent(PassedCount.ToString(), Icons.s_SuccessImg, succeededTooltip);
            var failedTooltip = string.Format("Show tests that failed\n{0} failed\n{1} inconclusive", m_FailedCount, m_InconclusiveCount);
            m_FailedBtn = new GUIContent(FailedCount.ToString(), Icons.s_FailImg, failedTooltip);
            var notRunTooltip = string.Format("Show tests that didn't run\n{0} didn't run\n{1} skipped or ignored", m_NotRunCount, m_SkippedCount);
            m_NotRunBtn = new GUIContent(NotRunCount.ToString(), Icons.s_UnknownImg, notRunTooltip);
        }

        public void Draw()
        {
            EditorGUI.BeginChangeCheck();
            if (m_SearchString == null)
            {
                m_SearchString = "";
            }
            m_SearchString = EditorGUILayout.ToolbarSearchField(m_SearchString);
            if (EditorGUI.EndChangeCheck() && SearchStringChanged != null)
            {
                SearchStringChanged(m_SearchString);
                if (String.IsNullOrEmpty(m_SearchString))
                    SearchStringCleared();
            }

            if (availableCategories != null && availableCategories.Any())
            {
                TestRunnerGUI.CategorySelectionDropDown(BuildCategorySelectionProvider());
            }
            else
            {
                EditorGUILayout.Popup(0, new[] { "<No categories available>" }, EditorStyles.toolbarDropDown, GUILayout.MaxWidth(150));
            }

            EditorGUI.BeginChangeCheck();
            if (m_SucceededBtn != null)
            {
                PassedHidden = !GUILayout.Toggle(!PassedHidden, m_SucceededBtn, EditorStyles.toolbarButton, GUILayout.MaxWidth(GetMaxWidth(PassedCount)));
            }
            if (m_FailedBtn != null)
            {
                FailedHidden = !GUILayout.Toggle(!FailedHidden, m_FailedBtn, EditorStyles.toolbarButton, GUILayout.MaxWidth(GetMaxWidth(FailedCount)));
            }
            if (m_NotRunBtn != null)
            {
                NotRunHidden = !GUILayout.Toggle(!NotRunHidden, m_NotRunBtn, EditorStyles.toolbarButton, GUILayout.MaxWidth(GetMaxWidth(NotRunCount)));
            }

            if (EditorGUI.EndChangeCheck() && RebuildTestList != null)
            {
                RebuildTestList();
            }
        }

        public void OnModeGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                var testPlatformItemProvider = BuildTestPlatformSelectionProvider();
                TestRunnerGUI.TestPlatformSelectionDropDown(testPlatformItemProvider);

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (SelectedTestPlatform.PlatformTarget == TestPlatformTarget.Editor)
                {
                    var editModeEnabled = (SelectedTestMode & RequirePlaymodeMode.TestsNotRequiringPlaymodeInEditor) ==
                        RequirePlaymodeMode.TestsNotRequiringPlaymodeInEditor;
                    var editModeState = GUILayout.Toggle(editModeEnabled, "EditMode");
                    if (editModeState != editModeEnabled)
                    {
                        SelectedTestMode = editModeState ?
                            RequirePlaymodeMode.TestsNotRequiringPlaymodeInEditor | RequirePlaymodeMode.TestsRequiringPlaymodeInEditor :
                            RequirePlaymodeMode.TestsRequiringPlaymodeInEditor;
                        UpdateTestTreeRoots();
                    }

                    EditorGUILayout.Space();

                    var requirePlayModeEnabled =
                        (SelectedTestMode & RequirePlaymodeMode.TestsRequiringPlaymodeInEditor) ==
                        RequirePlaymodeMode.TestsRequiringPlaymodeInEditor;
                    var playModeState = GUILayout.Toggle(requirePlayModeEnabled, "PlayMode");
                    if (playModeState != requirePlayModeEnabled)
                    {
                        SelectedTestMode = playModeState ?
                            RequirePlaymodeMode.TestsNotRequiringPlaymodeInEditor | RequirePlaymodeMode.TestsRequiringPlaymodeInEditor :
                            RequirePlaymodeMode.TestsNotRequiringPlaymodeInEditor;
                        UpdateTestTreeRoots();
                    }
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    GUILayout.FlexibleSpace();
                    var exportTests = GUILayout.Button(EditorGUIUtility.TrTextContent("Export tests", "Build and export a player with tests"),
                        EditorStyles.toolbarButton);
                    if (exportTests)
                    {
                        BuildPlayer();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        ISelectionDropDownContentProvider BuildTestPlatformSelectionProvider()
        {
            var selectableTestPlatformItems = TestRunnerUIFilterUtility.GetTestPlatformSelectables(out var separatorIndices);
            var itemProvider = new GenericItemContentProvider<TestPlatformSelection>(SelectedTestPlatform, selectableTestPlatformItems,
                separatorIndices, newSelection =>
                {
                    SelectedTestPlatform = newSelection;
                    UpdateTestTreeRoots();
                });

            return itemProvider;
        }

        ISelectionDropDownContentProvider BuildCategorySelectionProvider()
        {
            var itemProvider = new MultiValueContentProvider<string>(availableCategories, selectedCategories,
                (categories) =>
                {
                    selectedCategories = categories;
                    UpdateTestTreeRoots();
                });

            return itemProvider;
        }

        private static int GetMaxWidth(int count)
        {
            if (count < 10)
                return 33;
            return count < 100 ? 40 : 47;
        }

        public void Clear()
        {
            PassedHidden = false;
            FailedHidden = false;
            NotRunHidden = false;
            selectedCategories = new string[0];
            m_SearchString = "";
            if (SearchStringChanged != null)
            {
                SearchStringChanged(m_SearchString);
            }
            if (SearchStringCleared != null)
            {
                SearchStringCleared();
            }

            SelectedTestMode = k_DefaultRequirePlaymodeModeSelection;
        }

        public void ValidateTestPlatformSelection()
        {
            if (!IsTestPlatformAvailable(SelectedTestPlatform.PlatformTarget, SelectedTestPlatform.CustomTargetName))
            {
                Debug.Log("Test Runner view has been reset to show the Editor platform as the previously selected one " +
                    $"'{SelectedTestPlatform.CustomTargetName}' is not available anymore.");
                SelectedTestPlatform = default;
            }
        }

        bool IsTestPlatformAvailable(TestPlatformTarget platform, string name)
        {
            switch (platform)
            {
                case TestPlatformTarget.CustomPlayer:
                    return Array.IndexOf(TestRunnerApi.GetPlayerBuilderNames(), name) >= 0;
                case TestPlatformTarget.CustomRunner:
                    return Array.IndexOf(TestRunnerApi.GetCustomRunnerNames(), name) >= 0;
                default:
                    return true;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.GUI.TestAssets;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    [Serializable]
    internal class TestListGUI
    {
        private static readonly GUIContent s_GUIRunSelectedTests = EditorGUIUtility.TrTextContent("Run Selected", "Run selected test(s)");
        private static readonly GUIContent s_GUIRunAllTests = EditorGUIUtility.TrTextContent("Run All", "Run all tests");
        private static readonly GUIContent s_GUIRerunFailedTests = EditorGUIUtility.TrTextContent("Rerun Failed", "Rerun all failed tests");
        private static readonly GUIContent s_GUIRun = EditorGUIUtility.TrTextContent("Run");
        private static readonly GUIContent s_GUIRunUntilFailed = EditorGUIUtility.TrTextContent("Run Until Failed");
        private static readonly GUIContent s_GUIRun100Times = EditorGUIUtility.TrTextContent("Run 100 times");
        private static readonly GUIContent s_GUIOpenTest = EditorGUIUtility.TrTextContent("Open source code");
        private static readonly GUIContent s_GUIOpenErrorLine = EditorGUIUtility.TrTextContent("Open error line");
        private static readonly GUIContent s_GUIClearResults = EditorGUIUtility.TrTextContent("Clear Results", "Clear all test results");
        private static readonly GUIContent s_SaveResults = EditorGUIUtility.TrTextContent("Export Results", "Save the latest test results to a file");

        [SerializeField]
        private TestRunnerWindow m_Window;
        [NonSerialized]
        private TestRunnerApi m_TestRunnerApi;

        [NonSerialized]
        internal TestMode m_TestMode;

        [NonSerialized]
        internal bool m_RunOnPlatform;


        public Dictionary<string, TestTreeViewItem> filteredTree { get; set; }
        public List<TestRunnerResult> newResultList
        {
            get { return m_NewResultList; }
            set
            {
                m_NewResultList = value;
                m_ResultByKey = null;
            }
        }

        [SerializeField]
        private List<TestRunnerResult> m_NewResultList = new List<TestRunnerResult>();

        private Dictionary<string, TestRunnerResult> m_ResultByKey;
        internal Dictionary<string, TestRunnerResult> ResultsByKey
        {
            get
            {
                if (m_ResultByKey == null)
                {
                    try
                    {
                        m_ResultByKey = newResultList.ToDictionary(k => k.uniqueId);
                    }
                    catch (Exception ex)
                    {
                        // Reset the results, so we do not lock the editor in giving errors on this on every frame.
                        newResultList = new List<TestRunnerResult>();
                        throw ex;
                    }
                }

                return m_ResultByKey;
            }
        }

        [SerializeField]
        private string m_ResultText;
        [SerializeField]
        private string m_ResultStacktrace;

        private TreeViewController m_TestListTree;
        [SerializeField]
        internal TreeViewState m_TestListState;
        [SerializeField]
        internal TestRunnerUIFilter m_TestRunnerUIFilter = new TestRunnerUIFilter();

        private Vector2 m_TestInfoScroll, m_TestListScroll;
        private List<TestRunnerResult> m_QueuedResults = new List<TestRunnerResult>();
        private ITestResultAdaptor m_LatestTestResults;

        public TestListGUI()
        {
            MonoCecilHelper = new MonoCecilHelper();
            AssetsDatabaseHelper = new AssetsDatabaseHelper();

            GuiHelper = new GuiHelper(MonoCecilHelper, AssetsDatabaseHelper);
        }

        private IMonoCecilHelper MonoCecilHelper { get; set; }
        private IAssetsDatabaseHelper AssetsDatabaseHelper { get; set; }
        private IGuiHelper GuiHelper { get; set; }

        public void PrintHeadPanel()
        {
            if (m_RunOnPlatform)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label("Running on " + EditorUserBuildSettings.activeBuildTarget);
                EditorGUILayout.EndHorizontal();
            }

            using (new EditorGUI.DisabledScope(m_TestListTree == null || IsBusy()))
            {
                m_TestRunnerUIFilter.OnModeGUI();
                DrawFilters();
            }
        }

        public void PrintBottomPanel()
        {
            using (new EditorGUI.DisabledScope(m_TestListTree == null || IsBusy()))
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    using (new EditorGUI.DisabledScope(m_LatestTestResults == null))
                    {
                        if (GUILayout.Button(s_SaveResults))
                        {
                            var filePath = EditorUtility.SaveFilePanel(s_SaveResults.text, "",
                                $"TestResults_{DateTime.Now:yyyyMMdd_HHmmss}.xml", "xml");
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                TestRunnerApi.SaveResultToFile(m_LatestTestResults, filePath);
                            }

                            GUIUtility.ExitGUI();
                        }
                    }

                    if (GUILayout.Button(s_GUIClearResults))
                    {
                        foreach (var result in newResultList)
                        {
                            result.Clear();
                        }

                        m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
                        Reload();
                        GUIUtility.ExitGUI();
                    }

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(m_TestRunnerUIFilter.FailedCount == 0))
                    {
                        if (GUILayout.Button(s_GUIRerunFailedTests))
                        {
                            RunTests(RunFilterType.RunFailed);
                            GUIUtility.ExitGUI();
                        }
                    }

                    using (new EditorGUI.DisabledScope(m_TestListTree == null || !m_TestListTree.HasSelection()))
                    {
                        if (GUILayout.Button(s_GUIRunSelectedTests))
                        {
                            RunTests(RunFilterType.RunSelected);
                            GUIUtility.ExitGUI();
                        }
                    }

                    if (GUILayout.Button(s_GUIRunAllTests))
                    {
                        RunTests(RunFilterType.RunAll);
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            m_TestRunnerUIFilter.Draw();
            EditorGUILayout.EndHorizontal();
        }

        public bool HasTreeData()
        {
            return m_TestListTree != null;
        }

        public void RenderTestList()
        {
            if (m_TestListTree == null)
            {
                GUILayout.Label("Loading...");
                return;
            }

            m_TestListScroll = EditorGUILayout.BeginScrollView(m_TestListScroll,
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000));

            if (m_TestListTree.data.root == null || m_TestListTree.data.rowCount == 0 || (!m_TestListTree.isSearching && !m_TestListTree.data.GetItem(0).hasChildren))
            {
                if (m_TestRunnerUIFilter.IsFiltering)
                {
                    var notMatchFoundStyle = new GUIStyle("label");
                    notMatchFoundStyle.normal.textColor = Color.red;
                    notMatchFoundStyle.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("No match found", notMatchFoundStyle);
                    if (GUILayout.Button("Clear filters"))
                    {
                        m_TestRunnerUIFilter.Clear();
                        UpdateTestTree();
                        m_Window.Repaint();
                    }
                }
                RenderNoTestsInfo();
            }
            else
            {
                var treeRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                var treeViewKeyboardControlId = GUIUtility.GetControlID(FocusType.Keyboard);

                m_TestListTree.OnGUI(treeRect, treeViewKeyboardControlId);
            }

            m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
            EditorGUILayout.EndScrollView();
        }

        private void RenderNoTestsInfo()
        {
            var testScriptAssetsCreator = new TestScriptAssetsCreator();
            if (!testScriptAssetsCreator.ActiveFolderContainsTestAssemblyDefinition())
            {
                var noTestsText = "No tests to show.";

                if (!PlayerSettings.playModeTestRunnerEnabled)
                {
                    const string testsMustLiveInCustomTestAssemblies =
                        "Test scripts can be added to assemblies referencing the \"nunit.framework.dll\" library " +
                        "or folders with Assembly Definition References targeting \"UnityEngine.TestRunner\" or \"UnityEditor.TestRunner\".";

                    noTestsText += Environment.NewLine + testsMustLiveInCustomTestAssemblies;
                }

                EditorGUILayout.HelpBox(noTestsText, MessageType.Info);
                if (GUILayout.Button("Create a new Test Assembly Folder in the active path."))
                {
                    testScriptAssetsCreator.AddNewFolderWithTestAssemblyDefinition(m_TestMode == TestMode.EditMode);
                }
            }

            const string notTestAssembly = "Test Scripts can only be created inside test assemblies.";
            const string createTestScriptInCurrentFolder = "Create a new Test Script in the active path.";
            var canAddTestScriptAndItWillCompile = testScriptAssetsCreator.TestScriptWillCompileInActiveFolder();

            using (new EditorGUI.DisabledScope(!canAddTestScriptAndItWillCompile))
            {
                var createTestScriptInCurrentFolderGUI = !canAddTestScriptAndItWillCompile
                    ? new GUIContent(createTestScriptInCurrentFolder, notTestAssembly)
                    : new GUIContent(createTestScriptInCurrentFolder);

                if (GUILayout.Button(createTestScriptInCurrentFolderGUI))
                {
                    testScriptAssetsCreator.AddNewTestScript();
                }
            }
        }

        public void RenderDetails(float width)
        {
            m_TestInfoScroll = EditorGUILayout.BeginScrollView(m_TestInfoScroll, GUILayout.ExpandWidth(true));
            var resultTextHeight = TestRunnerWindow.Styles.info.CalcHeight(new GUIContent(m_ResultText), width);
            EditorGUILayout.SelectableLabel(m_ResultText, TestRunnerWindow.Styles.info,
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true),
                GUILayout.MinHeight(resultTextHeight));
            EditorGUILayout.EndScrollView();
        }

        public void Reload()
        {
            if (m_TestListTree != null)
            {
                m_TestListTree.ReloadData();
                UpdateQueuedResults();
            }
        }

        public void Repaint()
        {
            if (m_TestListTree == null || m_TestListTree.data.root == null)
            {
                return;
            }

            m_TestListTree.Repaint();
            if (m_TestListTree.data.rowCount == 0)
                m_TestListTree.SetSelection(new int[0], false);
            TestSelectionCallback(m_TestListState.selectedIDs.ToArray());
        }

        public void Init(TestRunnerWindow window, ITestAdaptor rootTest)
        {
            Init(window, new[] {rootTest});
        }

        private void Init(TestRunnerWindow window, ITestAdaptor[] rootTests)
        {
            if (m_Window == null)
            {
                m_Window = window;
            }

            if (m_TestListTree == null)
            {
                if (m_TestListState == null)
                {
                    m_TestListState = new TreeViewState();
                }
                if (m_TestListTree == null)
                    m_TestListTree = new TreeViewController(m_Window, m_TestListState);

                m_TestListTree.deselectOnUnhandledMouseDown = false;

                m_TestListTree.selectionChangedCallback += TestSelectionCallback;
                m_TestListTree.itemDoubleClickedCallback += TestDoubleClickCallback;
                m_TestListTree.contextClickItemCallback += TestContextClickCallback;

                var testListTreeViewDataSource = new TestListTreeViewDataSource(m_TestListTree, this, rootTests);

                if (!newResultList.Any())
                    testListTreeViewDataSource.ExpandTreeOnCreation();

                m_TestListTree.Init(new Rect(),
                    testListTreeViewDataSource,
                    new TestListTreeViewGUI(m_TestListTree),
                    null);
            }

            m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
            m_TestRunnerUIFilter.RebuildTestList = () => m_TestListTree.ReloadData();
            m_TestRunnerUIFilter.UpdateTestTreeRoots = UpdateTestTree;
            m_TestRunnerUIFilter.SearchStringChanged = s => m_TestListTree.ReloadData();
            m_TestRunnerUIFilter.SearchStringCleared = () => FrameSelection();
        }

        public void UpdateResult(TestRunnerResult result)
        {
            if (!HasTreeData())
            {
                m_QueuedResults.Add(result);
                return;
            }


            if (!ResultsByKey.TryGetValue(result.uniqueId, out var testRunnerResult))
            {
                // Add missing result due to e.g. changes in code for uniqueId due to change of package version.
                m_NewResultList.Add(result);
                ResultsByKey[result.uniqueId] = result;
                testRunnerResult = result;
            }

            testRunnerResult.Update(result);
            Repaint();
            m_Window.Repaint();
        }

        public void RunFinished(ITestResultAdaptor results)
        {
            m_LatestTestResults = results;
            UpdateTestTree();
        }

        private void UpdateTestTree()
        {
            var testList = this;
            if (m_TestRunnerApi == null)
            {
                m_TestRunnerApi= ScriptableObject.CreateInstance<TestRunnerApi>();
            }

            m_TestRunnerApi.RetrieveTestList(GetExecutionSettings(), rootTest =>
            {
                testList.UpdateTestTree(new[] { rootTest });
                testList.Reload();
            });
        }

        public void UpdateTestTree(ITestAdaptor[] tests)
        {
            if (!HasTreeData())
            {
                return;
            }

            (m_TestListTree.data as TestListTreeViewDataSource).UpdateRootTest(tests);

            m_TestListTree.ReloadData();
            Repaint();
            m_Window.Repaint();
        }

        private void UpdateQueuedResults()
        {
            foreach (var testRunnerResult in m_QueuedResults)
            {
                if (ResultsByKey.TryGetValue(testRunnerResult.uniqueId, out var existingResult))
                {
                    existingResult.Update(testRunnerResult);
                }
            }
            m_QueuedResults.Clear();
            TestSelectionCallback(m_TestListState.selectedIDs.ToArray());
            m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
            Repaint();
            m_Window.Repaint();
        }

        internal void TestSelectionCallback(int[] selected)
        {
            if (m_TestListTree != null && selected.Length == 1)
            {
                if (m_TestListTree != null)
                {
                    var node = m_TestListTree.FindItem(selected[0]);
                    if (node is TestTreeViewItem)
                    {
                        var test = node as TestTreeViewItem;
                        m_ResultText = test.GetResultText();
                        m_ResultStacktrace = test.result.stacktrace;
                    }
                }
            }
            else if (selected.Length == 0)
            {
                m_ResultText = "";
            }
        }

        private void TestDoubleClickCallback(int id)
        {
            if (IsBusy())
                return;

            RunTests(RunFilterType.RunSpecific, id);
            GUIUtility.ExitGUI();
        }

        private void RunTests(RunFilterType runFilter, params int[] specificTests)
        {
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError("Fix compilation issues before running tests");
                return;
            }

            var filters = ConstructFilter(runFilter, specificTests);
            if (filters == null)
            {
                return;
            }

            foreach (var filter in filters)
            {
                filter.ClearResults(newResultList.OfType<UITestRunnerFilter.IClearableResult>().ToDictionary(result => result.Id));
            }

            var testFilters = filters.Select(filter => new Filter
            {
                testMode = m_TestMode,
                assemblyNames = filter.assemblyNames,
                categoryNames = filter.categoryNames,
                groupNames = filter.groupNames,
                testNames = filter.testNames,
            }).ToArray();

#if UNITY_2022_2_OR_NEWER
            var executionSettings =
                CreateExecutionSettings(m_RunOnPlatform ? EditorUserBuildSettings.activeBuildTarget : null,
                    testFilters);
#else
            var executionSettings =
                CreateExecutionSettings(m_RunOnPlatform ? EditorUserBuildSettings.activeBuildTarget : (BuildTarget?)null,
                    testFilters);
#endif     
            
            if (m_TestRunnerApi == null)
            {
                m_TestRunnerApi= ScriptableObject.CreateInstance<TestRunnerApi>();
            }

            m_TestRunnerApi.Execute(executionSettings);

            if (executionSettings.targetPlatform != null)
            {
                GUIUtility.ExitGUI();
            }
        }

        private void TestContextClickCallback(int id)
        {
            if (id == 0)
                return;

            var m = new GenericMenu();
            var multilineSelection = m_TestListState.selectedIDs.Count > 1;

            if (!multilineSelection)
            {
                var testNode = GetSelectedTest();
                var isNotSuite = !testNode.IsGroupNode;
                if (isNotSuite)
                {
                    if (!string.IsNullOrEmpty(m_ResultStacktrace))
                    {
                        m.AddItem(s_GUIOpenErrorLine,
                            false,
                            data =>
                            {
                                if (!GuiHelper.OpenScriptInExternalEditor(m_ResultStacktrace))
                                {
                                    GuiHelper.OpenScriptInExternalEditor(testNode.type, testNode.method);
                                }
                            },
                            "");
                    }

                    m.AddItem(s_GUIOpenTest,
                        false,
                        data => GuiHelper.OpenScriptInExternalEditor(testNode.type, testNode.method),
                        "");
                    m.AddSeparator("");
                }
            }

            if (!IsBusy())
            {
                m.AddItem(multilineSelection ? s_GUIRunSelectedTests : s_GUIRun,
                    false,
                    data => RunTests(RunFilterType.RunSelected),
                    "");
            }
            else
                m.AddDisabledItem(multilineSelection ? s_GUIRunSelectedTests : s_GUIRun, false);

            m.ShowAsContext();
        }

        private enum RunFilterType
        {
            RunAll,
            RunSelected,
            RunFailed,
            RunSpecific
        }

        private struct FilterConstructionStep
        {
            public int Id;
            public TreeViewItem Item;
        }

        private UITestRunnerFilter[] ConstructFilter(RunFilterType runFilter, int[] specificTests = null)
        {
            if (runFilter == RunFilterType.RunAll && !m_TestRunnerUIFilter.IsFiltering)
            {
                // Shortcut for RunAll, which will not trigger any explicit tests
                return new[] {new UITestRunnerFilter()};
            }

            var includedIds = GetIdsIncludedInRunFilter(runFilter, specificTests);
            var visitedItems = new HashSet<int>();
            var openItems = new Stack<FilterConstructionStep>();
            var testsToRun = new List<string>();

            foreach (var id in includedIds)
            {
                if (visitedItems.Contains(id))
                    continue;
                openItems.Push(new FilterConstructionStep
                {
                    Id = id,
                    Item = m_TestListTree.FindItem(id)
                });
                visitedItems.Add(id);
                while (openItems.Count > 0)
                {
                    var top = openItems.Pop();
                    var item = top.Item;
                    if (item.hasChildren)
                    {
                        foreach (var child in item.children)
                        {
                            var childId = child.id;
                            if (!visitedItems.Contains(childId))
                            {
                                var testItem = (TestTreeViewItem)child;
                                if (testItem.m_Test.RunState == RunState.Explicit)
                                {
                                    // Do not add explicit tests if a ancestor is selected.
                                    continue;
                                }

                                openItems.Push(new FilterConstructionStep
                                {
                                    Id = childId,
                                    Item = child
                                });
                                visitedItems.Add(childId);
                            }
                        }
                    }
                    else
                    {
                        var testItem = (TestTreeViewItem)item;
                        if (runFilter == RunFilterType.RunFailed)
                        {
                            ResultsByKey.TryGetValue(testItem.UniqueName, out var testRunnerResult);
                            if (testRunnerResult?.resultStatus == TestRunnerResult.ResultStatus.Failed)
                            {
                                continue;
                            }
                        }

                        testsToRun.Add(testItem.FullName);
                    }
                }
            }

            if (testsToRun.Count == 0)
            {
                return null;
            }

            return new[]
            {
                new UITestRunnerFilter
                {
                    testNames = testsToRun.ToArray(),
                    categoryNames = m_TestRunnerUIFilter.CategoryFilter
                }
            };
        }

        private IEnumerable<int> GetIdsIncludedInRunFilter(RunFilterType runFilter, int[] specificTests)
        {
            switch (runFilter)
            {
                case RunFilterType.RunSelected:
                    return m_TestListState.selectedIDs;
                case RunFilterType.RunSpecific:
                    if (specificTests == null)
                    {
                        throw new ArgumentNullException(
                            $"For {nameof(RunFilterType.RunSpecific)}, the {nameof(specificTests)} argument must not be null.");
                    }

                    return specificTests;
                default:
                    return m_TestListTree.GetRowIDs();
            }
        }

        private TestTreeViewItem GetSelectedTest()
        {
            foreach (var lineId in m_TestListState.selectedIDs)
            {
                var line = m_TestListTree.FindItem(lineId);
                if (line is TestTreeViewItem)
                {
                    return line as TestTreeViewItem;
                }
            }
            return null;
        }

        private void FrameSelection()
        {
            if (m_TestListTree.HasSelection())
            {
                var firstClickedID = m_TestListState.selectedIDs.First() == m_TestListState.lastClickedID ? m_TestListState.selectedIDs.Last() : m_TestListState.selectedIDs.First();
                m_TestListTree.Frame(firstClickedID, true, false);
            }
        }

        public void RebuildUIFilter()
        {
            m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
            if (m_TestRunnerUIFilter.IsFiltering)
            {
                m_TestListTree.ReloadData();
            }
        }

        private static bool IsBusy()
        {
            return TestRunnerApi.IsRunActive() || EditorApplication.isCompiling || EditorApplication.isPlaying;
        }

        public ExecutionSettings GetExecutionSettings()
        {
            var filter = new Filter
            {
                testMode = m_TestMode
            };

#if UNITY_2022_2_OR_NEWER
            return CreateExecutionSettings(m_RunOnPlatform ? EditorUserBuildSettings.activeBuildTarget : null, filter);
#else
            return CreateExecutionSettings(m_RunOnPlatform ? EditorUserBuildSettings.activeBuildTarget : (BuildTarget?)null, filter);
#endif            
        }

        private static ExecutionSettings CreateExecutionSettings(BuildTarget? buildTarget, params Filter[] filters)
        {
            return new ExecutionSettings(filters)
            {
                targetPlatform = buildTarget,
            };
        }
    }
}

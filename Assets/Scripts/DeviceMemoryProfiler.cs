using System;
using System.Collections.Generic;
using UnityEngine;

namespace Anexas.DeviceTools
{
    ///<summary>
    /// This class lists Unity Objects and how much memory they consume.
    ///</summary>
    public class DeviceMemoryProfiler : MonoBehaviour
    {
        private static DeviceMemoryProfiler _instance;
        public static DeviceMemoryProfiler Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject deviceMemoryProfiler = new GameObject("DeviceMemoryProfiler");
                    _instance = deviceMemoryProfiler.AddComponent<DeviceMemoryProfiler>();
                    GameObject.DontDestroyOnLoad(deviceMemoryProfiler);
                }
                return _instance;
            }
        }

        private bool _show = false;
        public bool Show
        {
            get { return _show; }
            set { _show = value; }
        }

        [Serializable]
        private struct UnityObjectData
        {
            public string name;
            public string type;
            public long size;
            public int originalIndex;
            public string additionalData;
            public bool additionalDataHighlight;

            public static int sortDirection = DEFAULT_SORT_DIRECTION;

            public static int SortByName(UnityObjectData objA, UnityObjectData objB)
            {
                if (sortDirection == 0)
                {
                    return objA.name.CompareTo(objB.name);
                }

                return objB.name.CompareTo(objA.name);
            }

            public static int SortBySize(UnityObjectData objA, UnityObjectData objB)
            {
                if (sortDirection == 0)
                {
                    return (objA.size < objB.size ? -1 : ((objA.size == objB.size) ? 0 : 1));
                }

                return (objB.size < objA.size ? -1 : ((objB.size == objA.size) ? 0 : 1));
            }
        }

        private string _className = typeof(UnityEngine.Object).FullName;
        private string _assemblyName = "UnityEngine.CoreModule";

        private List<UnityObjectData> _objectDataList = new List<UnityObjectData>();

        private long _totalBytes = 0;
        private int _pageNum = 0;

        private const int DEFAULT_PAGE_SIZE_INDEX = 1;
        private int _pageSizeIndex = DEFAULT_PAGE_SIZE_INDEX;
        private static readonly string[] _pageSizeStrings = { "10", "100", "500" };
        private static readonly int[] _pageSizeArray = { 10, 100, 500 };

        private const int DEFAULT_UNITS = 2;
        private int _units = DEFAULT_UNITS;
        private static readonly string[] _unitsStrings = { "Bytes", "KB", "MB", "GB" };
        private static readonly long[] _unitsDivide = { 1, 1024, 1048576, 1073741824 };

        private const int DEFAULT_SORT_BY = 1;
        private int _sortBy = DEFAULT_SORT_BY;
        private static readonly string[] _sortByStrings = { "Name", "Size" };

        private const int DEFAULT_SORT_DIRECTION = 1;
        private static int SortDirection
        {
            get { return UnityObjectData.sortDirection; }
            set { UnityObjectData.sortDirection = value; }
        }
        private static readonly string[] _sortDirectionStrings = { "\u25B2", "\u25BC" };

        private Vector2 _scrollPosition;

        private void UpdateObjectDataList()
        {
            Type type = Type.GetType(_className + ", " + _assemblyName);

            if (type == null)
            {
                return;
            }

            UnityEngine.Object[] objects = UnityEngine.Resources.FindObjectsOfTypeAll(type);
            _objectDataList.Clear();
            _totalBytes = 0;

            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                UnityObjectData uod = new UnityObjectData();
                uod.type = obj.GetType().ToString();
                uod.name = obj.name;
#if ENABLE_PROFILER
                uod.size = (UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(obj));
#endif
                if (obj is Material)
                {
                    Material mat = (obj as Material);
                    if (mat.shader != null)
                    {
                        uod.additionalData = mat.shader.name;
                    }
                }

                if (obj is Renderer)
                {
                    Renderer rend = (obj as Renderer);
                    if (rend.sharedMaterial != null)
                    {
                        uod.additionalData = rend.sharedMaterial.name;
                        if (rend.sharedMaterials.Length > 1)
                        {
                            uod.additionalDataHighlight = true;
                        }
                    }
                }

                uod.originalIndex = i;
                _objectDataList.Add(uod);

                _totalBytes += uod.size;
            }

            SortObjectDataList();
        }

        private void SortObjectDataList()
        {
            if (_sortBy == 1)
            {
                _objectDataList.Sort(UnityObjectData.SortBySize);
                return;
            }

            _objectDataList.Sort(UnityObjectData.SortByName);
        }

        private void OnGUI()
        {
            if (!_show)
            {
                return;
            }

            using (var verticalScope = new GUILayout.VerticalScope("box"))
            {
                using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                {
                    _className = GUILayout.TextField(_className);
                    _assemblyName = GUILayout.TextField(_assemblyName);
                    if (GUILayout.Button("Check"))
                    {
                        UpdateObjectDataList();
                        _pageNum = 0;
                    }
                }

                if (_objectDataList.Count > 0)
                {
                    using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                    {
                        GUILayout.Label("Count: " + _objectDataList.Count.ToString("N0"));
                        GUILayout.Label("Total: " + ((float)_totalBytes / (float)_unitsDivide[_units]).ToString("N" + _units * 2));
                        _units = GUILayout.Toolbar(_units, _unitsStrings);
                    }

                    using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                    {
                        _pageSizeIndex = GUILayout.Toolbar(_pageSizeIndex, _pageSizeStrings);

                        using (var horizontalScope2 = new GUILayout.HorizontalScope("box"))
                        {
                            Color oldBackgroundColor = GUI.backgroundColor;

                            int totalPages = (_objectDataList.Count / _pageSizeArray[_pageSizeIndex]) + (((_objectDataList.Count % _pageSizeArray[_pageSizeIndex]) > 0) ? 1 : 0);
                            if (totalPages > 0)
                            {
                                if (_pageNum >= totalPages)
                                    _pageNum = totalPages - 1;

                                if (_pageNum == 0)
                                    GUI.backgroundColor = new Color(1, 0.5f, 0.5f);
                                if (GUILayout.Button("\u25C0"))
                                {
                                    if (_pageNum > 0)
                                        _pageNum--;
                                }
                                GUI.backgroundColor = oldBackgroundColor;

                                GUILayout.Label((_pageNum + 1).ToString());
                                GUILayout.Label("/");
                                GUILayout.Label(totalPages.ToString());

                                if (_pageNum == (totalPages - 1))
                                    GUI.backgroundColor = new Color(1, 0.5f, 0.5f);
                                if (GUILayout.Button("\u25B6"))
                                {
                                    if (_pageNum < (totalPages - 1))
                                        _pageNum++;
                                }
                                GUI.backgroundColor = oldBackgroundColor;
                            }
                        }

                        int oldSortBy = _sortBy;
                        _sortBy = GUILayout.Toolbar(_sortBy, _sortByStrings);
                        if (_sortBy != oldSortBy)
                        {
                            SortObjectDataList();
                        }

                        int oldSortDirection = SortDirection;
                        SortDirection = GUILayout.Toolbar(SortDirection, _sortDirectionStrings);
                        if (SortDirection != oldSortDirection)
                        {
                            SortObjectDataList();
                        }
                    }

                    int startNum = _pageNum * _pageSizeArray[_pageSizeIndex];
                    int endNum = startNum + _pageSizeArray[_pageSizeIndex];

                    if (endNum > _objectDataList.Count)
                        endNum = _objectDataList.Count;

                    using (var scrollViewScope = new GUILayout.ScrollViewScope(_scrollPosition))
                    {
                        _scrollPosition = scrollViewScope.scrollPosition;

                        for (int i = startNum; i < endNum; i++)
                        {
                            var obj = _objectDataList[i];
                            using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                            {
                                GUILayout.Label((i + 1).ToString() + ".");
                                GUILayout.Label(obj.type);
                                if (!string.IsNullOrWhiteSpace(obj.name))
                                {
                                    GUILayout.Label(obj.name);
                                }
                                else
                                {
                                    Color oldColor = GUI.color;
                                    GUI.color = new Color(1, 0.5f, 0);
                                    GUILayout.Label("<No Name>");
                                    GUI.color = oldColor;
                                }
                                GUILayout.Label(((float)obj.size / (float)_unitsDivide[_units]).ToString("N" + _units * 2));
                                Color oldCol = GUI.color;
                                if (obj.additionalDataHighlight)
                                    GUI.color = new Color(1, 1, 0);
                                GUILayout.Label(obj.additionalData);
                                GUI.color = oldCol;
                            }
                        }
                    }
                }
                else
                {
                    _pageNum = 0;
                    _pageSizeIndex = DEFAULT_PAGE_SIZE_INDEX;
                    _units = DEFAULT_UNITS;
                    _sortBy = DEFAULT_SORT_BY;
                    SortDirection = DEFAULT_SORT_DIRECTION;
                    _scrollPosition = Vector2.zero;
                }
            }
        }
    }
}
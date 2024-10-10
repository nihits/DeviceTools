using UnityEngine;
using UnityEngine.SceneManagement;

namespace Anexas.DeviceTools
{
    ///<summary>
    /// This class displays the Hierarchy Panel on Devices.
    ///</summary>
    public class DeviceHierarchy : MonoBehaviour
    {
        private static DeviceHierarchy _instance;
        public static DeviceHierarchy Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject deviceHierarchy = new GameObject("DeviceHierarchy");
                    _instance = deviceHierarchy.AddComponent<DeviceHierarchy>();
                    GameObject.DontDestroyOnLoad(deviceHierarchy);
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

        static private Scene EMPTY_SCENE = new Scene();

        static private string BLACK_LEFT = "\u25C0";
        static private string BLACK_RIGHT = "\u25B6";

        static private string WHITE_LEFT = "\u25C1";
        static private string WHITE_RIGHT = "\u25B7";

        static private string BLACKWHITE_RIGHT = "\u25B6\u25B7";

        private Scene _dontDestroyOnLoadScene;
        private Scene _currentScene = EMPTY_SCENE;
        private GameObject _currentGameObject = null;
        private Component _currentComponent = null;

        private void Awake()
        {
            GameObject _tempGo = new GameObject();
            DontDestroyOnLoad(_tempGo);
            _dontDestroyOnLoadScene = _tempGo.scene;
        }

        private void OnGUI()
        {
            if (!_show)
            {
                return;
            }

            using (var verticalScopeRoot = new GUILayout.VerticalScope("box"))
            {
                using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                {
                    GUIStyle labelStyle = GUI.skin.GetStyle("Label");
                    FontStyle oldFontStyle = labelStyle.fontStyle;
                    labelStyle.fontStyle = FontStyle.Bold;
                    GUILayout.Label("Hierarchy", labelStyle);
                    labelStyle.fontStyle = oldFontStyle;
                }

                if (_currentScene == EMPTY_SCENE || !_currentScene.IsValid() || string.IsNullOrEmpty(_currentScene.name))
                {
                    using (var verticalScope = new GUILayout.VerticalScope("box"))
                    {
                        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
                        {
                            Scene scene = SceneManager.GetSceneAt(sceneIndex);
                            using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                            {
                                string sceneName = scene.name;
                                GUIStyle labelStyle = GUI.skin.GetStyle("Label");
                                FontStyle oldFontStyle = labelStyle.fontStyle;
                                if (SceneManager.GetActiveScene() == scene)
                                {
                                    labelStyle.fontStyle = FontStyle.Bold;
                                }
                                GUILayout.Label(sceneName, labelStyle);
                                labelStyle.fontStyle = oldFontStyle;
                                if (GUILayout.Button(WHITE_RIGHT))
                                {
                                    _currentScene = scene;
                                    _currentGameObject = null;
                                    _currentComponent = null;
                                    return;
                                }
                            }
                        }

                        using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                        {
                            GUILayout.Label(_dontDestroyOnLoadScene.name);
                            if (GUILayout.Button(WHITE_RIGHT))
                            {
                                _currentScene = _dontDestroyOnLoadScene;
                                _currentGameObject = null;
                                _currentComponent = null;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (_currentGameObject == null)
                    {
                        using (var verticalScope = new GUILayout.VerticalScope("box"))
                        {
                            using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                            {
                                if (GUILayout.Button(WHITE_LEFT))
                                {
                                    _currentScene = EMPTY_SCENE;
                                    _currentGameObject = null;
                                    _currentComponent = null;
                                    return;
                                }
                                GUIStyle labelStyle = GUI.skin.GetStyle("Label");
                                FontStyle oldFontStyle = labelStyle.fontStyle;
                                labelStyle.fontStyle = FontStyle.Bold;
                                GUILayout.Label(_currentScene.name);
                                labelStyle.fontStyle = oldFontStyle;
                            }

                            GameObject[] gameObjects = _currentScene.GetRootGameObjects();
                            for (int goIndex = 0; goIndex < gameObjects.Length; goIndex++)
                            {
                                GameObject gameObject = gameObjects[goIndex];
                                using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                                {
                                    Color oldColor = GUI.color;
                                    if (!gameObject.activeSelf)
                                    {
                                        GUI.color = Color.gray;
                                    }
                                    gameObject.SetActive(GUILayout.Toggle(gameObject.activeSelf, gameObject.name));
                                    GUI.color = oldColor;

                                    string buttonString = gameObject.transform.childCount > 0 ? BLACKWHITE_RIGHT : WHITE_RIGHT;
                                    if (GUILayout.Button(buttonString))
                                    {
                                        _currentGameObject = gameObject;
                                        _currentComponent = null;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var horizontalScopeGameObject = new GUILayout.HorizontalScope("box"))
                        {
                            using (var verticalScope = new GUILayout.VerticalScope("box"))
                            {
                                using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                                {
                                    Transform parentTransform = _currentGameObject.transform.parent;
                                    if (parentTransform == null)
                                    {
                                        if (GUILayout.Button(WHITE_LEFT))
                                        {
                                            _currentGameObject = null;
                                            _currentComponent = null;
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (GUILayout.Button(BLACK_LEFT))
                                        {
                                            _currentGameObject = parentTransform.gameObject;
                                            _currentComponent = null;
                                            return;
                                        }
                                    }
                                    GUIStyle toggleStyle = GUI.skin.GetStyle("Toggle");
                                    FontStyle oldFontStyle = toggleStyle.fontStyle;
                                    toggleStyle.fontStyle = FontStyle.Bold;
                                    Color oldColor = GUI.color;
                                    if (!_currentGameObject.activeSelf)
                                    {
                                        GUI.color = Color.gray;
                                    }
                                    _currentGameObject.SetActive(GUILayout.Toggle(_currentGameObject.activeSelf, _currentGameObject.name));
                                    GUI.color = oldColor;
                                    toggleStyle.fontStyle = oldFontStyle;
                                }

                                for (int goIndex = 0; goIndex < _currentGameObject.transform.childCount; goIndex++)
                                {
                                    GameObject gameObject = _currentGameObject.transform.GetChild(goIndex).gameObject;
                                    using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                                    {
                                        Color oldColor = GUI.color;
                                        if (!gameObject.activeSelf)
                                        {
                                            GUI.color = Color.gray;
                                        }
                                        gameObject.SetActive(GUILayout.Toggle(gameObject.activeSelf, gameObject.name));
                                        GUI.color = oldColor;

                                        string buttonString = gameObject.transform.childCount > 0 ? BLACKWHITE_RIGHT : WHITE_RIGHT;
                                        if (GUILayout.Button(buttonString))
                                        {
                                            _currentGameObject = gameObject;
                                            _currentComponent = null;
                                            return;
                                        }
                                    }
                                }
                            }
                            using (var verticalScope = new GUILayout.VerticalScope("box"))
                            {
                                for (int componentIndex = 0; componentIndex < _currentGameObject.GetComponentCount(); componentIndex++)
                                {
                                    Component component = _currentGameObject.GetComponentAtIndex(componentIndex);
                                    using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                                    {
                                        Behaviour behaviour = component as Behaviour;
                                        Renderer renderer = component as Renderer;
                                        if (behaviour != null)
                                        {
                                            Color oldColor = GUI.color;
                                            if (!behaviour.enabled)
                                            {
                                                GUI.color = Color.gray;
                                            }
                                            behaviour.enabled = (GUILayout.Toggle(behaviour.enabled, behaviour.GetType().Name));
                                            GUI.color = oldColor;
                                        }
                                        else if (renderer != null)
                                        {
                                            Color oldColor = GUI.color;
                                            if (!renderer.enabled)
                                            {
                                                GUI.color = Color.gray;
                                            }
                                            renderer.enabled = (GUILayout.Toggle(renderer.enabled, renderer.GetType().Name));
                                            GUI.color = oldColor;
                                        }
                                        else
                                        {
                                            GUILayout.Label(component.GetType().Name);
                                        }

                                        string buttonString = component == _currentComponent ? BLACK_RIGHT : WHITE_RIGHT;
                                        if (GUILayout.Button(buttonString))
                                        {
                                            _currentComponent = component;
                                            return;
                                        }
                                    }
                                }
                            }
                            if (_currentComponent != null)
                            {
                                // Show Transform, Renderer, MeshFilter, ParticleSystem, PostProcess?
                                if (_currentComponent is Transform)
                                {
                                    Transform tf = _currentComponent as Transform;
                                    using (var verticalScope = new GUILayout.VerticalScope("box"))
                                    {
                                        using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                                        {
                                            GUILayout.Label("x: " + tf.localPosition.x.ToString());
                                            GUILayout.Label("y: " + tf.localPosition.y.ToString());
                                            GUILayout.Label("z: " + tf.localPosition.z.ToString());
                                        }
                                        using (var horizontalScope = new GUILayout.HorizontalScope("box"))
                                        {
                                            GUILayout.Label("x: " + tf.localEulerAngles.x.ToString());
                                            GUILayout.Label("y: " + tf.localEulerAngles.y.ToString());
                                            GUILayout.Label("z: " + tf.localEulerAngles.z.ToString());
                                        }
                                    }
                                }
                                if (_currentComponent is Renderer)
                                {
                                    Renderer renderer = _currentComponent as Renderer;
                                    using (var verticalScope = new GUILayout.VerticalScope("box"))
                                    {
                                        Material[] materials = renderer.materials;
                                        for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                                        {
                                            Material material = materials[materialIndex];
                                            if (material != null)
                                            {
                                                GUIStyle labelStyle = GUI.skin.GetStyle("Label");
                                                FontStyle oldFontStyle = labelStyle.fontStyle;
                                                labelStyle.fontStyle = FontStyle.Bold;
                                                GUILayout.Label(material.name);
                                                labelStyle.fontStyle = oldFontStyle;
                                                if (material.shader != null)
                                                {
                                                    GUILayout.Label(material.shader.name);
                                                }
                                                if (material.mainTexture != null)
                                                {
                                                    GUILayout.Label(material.mainTexture.name);
                                                }
                                            }
                                        }
                                        Material[] sharedMaterials = renderer.sharedMaterials;
                                        for (int sharedMaterialIndex = 0; sharedMaterialIndex < materials.Length; sharedMaterialIndex++)
                                        {
                                            Material sharedMaterial = sharedMaterials[sharedMaterialIndex];
                                            GUIStyle labelStyle = GUI.skin.GetStyle("Label");
                                            FontStyle oldFontStyle = labelStyle.fontStyle;
                                            labelStyle.fontStyle = FontStyle.Bold;
                                            GUILayout.Label(sharedMaterial.name);
                                            labelStyle.fontStyle = oldFontStyle;
                                            if (sharedMaterial.shader != null)
                                            {
                                                GUILayout.Label(sharedMaterial.shader.name);
                                            }
                                            if (sharedMaterial.mainTexture != null)
                                            {
                                                GUILayout.Label(sharedMaterial.mainTexture.name);
                                            }
                                        }
                                    }
                                }
                                if (_currentComponent is MeshFilter)
                                {
                                    MeshFilter meshFilter = _currentComponent as MeshFilter;
                                    using (var verticalScope = new GUILayout.VerticalScope("box"))
                                    {
                                        GUILayout.Label(meshFilter.name);
                                        if (meshFilter.mesh != null)
                                        {
                                            GUILayout.Label(meshFilter.mesh.name);
                                        }
                                    }
                                }
                                if (_currentComponent is ParticleSystem)
                                {
                                    ParticleSystem particleSystem = _currentComponent as ParticleSystem;
                                    using (var verticalScope = new GUILayout.VerticalScope("box"))
                                    {
                                        GUIStyle labelStyle = GUI.skin.GetStyle("Label");
                                        FontStyle oldFontStyle = labelStyle.fontStyle;
                                        labelStyle.fontStyle = FontStyle.Bold;
                                        GUILayout.Label(particleSystem.name);
                                        labelStyle.fontStyle = oldFontStyle;

                                        ParticleSystem.MainModule particleSystemMainModule = particleSystem.main;
                                        GUILayout.Label(particleSystemMainModule.GetType().Name);
                                        GUILayout.Label("Duration: " + particleSystemMainModule.duration.ToString());

                                        ParticleSystem.EmissionModule particleSystemEmissionModule = particleSystem.emission;
                                        particleSystemEmissionModule.enabled = GUILayout.Toggle(particleSystemEmissionModule.enabled, particleSystemEmissionModule.GetType().Name);

                                        ParticleSystemRenderer particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                                        particleSystemRenderer.enabled = GUILayout.Toggle(particleSystemRenderer.enabled, particleSystemRenderer.GetType().Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
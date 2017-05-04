﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;

namespace DragonBones
{
    /**
     * @private
     */
    [CustomEditor(typeof(UnityArmatureComponent))]
    public class UnityArmatureEditor : Editor
    {
        [MenuItem("GameObject/DragonBones/Armature Object", false, 10)]
        private static void _createArmatureObjectMenuItem()
        {
            _createEmptyObject(_getSelectionParentTransform());
        }

        #region 右键JSON创建对应的Prefab
        [MenuItem("Assets/Create/DragonBones/Armature Object", true)]
        private static bool _createArmatureObjectFromJSONValidateMenuItem()
        {
            return _getDragonBonesJSONPaths().Count > 0;
        }

        [MenuItem("Assets/Create/DragonBones/Armature Object", false, 10)]
        private static void _createArmatureObjectFromJSONMenuItem()
        {
            var parentTransform = _getSelectionParentTransform();
            foreach (var dragonBonesJSONPath in _getDragonBonesJSONPaths())
            {
                var armatureComponent = _createEmptyObject(parentTransform);
                var dragonBonesJSON = AssetDatabase.LoadMainAssetAtPath(dragonBonesJSONPath) as TextAsset;

                _changeDragonBonesData(armatureComponent, dragonBonesJSON);
            }
        }

		[MenuItem("GameObject/DragonBones/Armature Object(UGUI)", false, 11)]
		private static void _createUGUIArmatureObjectMenuItem()
		{
			var armatureComponent = _createEmptyObject(_getSelectionParentTransform());
			armatureComponent.isUGUI=true;
			if(armatureComponent.GetComponentInParent<Canvas>()==null){
				var canvas = GameObject.Find("/Canvas");
				if(canvas){
					armatureComponent.transform.SetParent(canvas.transform);
				}
			}
			armatureComponent.transform.localScale = Vector2.one*100f;
			armatureComponent.transform.localPosition = Vector3.zero;
		}

		[MenuItem("Assets/Create/DragonBones/Armature Object(UGUI)", true)]
		private static bool _createUGUIArmatureObjectFromJSONValidateMenuItem()
		{
			return _getDragonBonesJSONPaths().Count > 0;
		}

		[MenuItem("Assets/Create/DragonBones/Armature Object(UGUI)", false, 11)]
		private static void _createNUGUIArmatureObjectFromJSOIMenuItem()
		{
			var parentTransform = _getSelectionParentTransform();
			foreach (var dragonBonesJSONPath in _getDragonBonesJSONPaths())
			{
				var armatureComponent = _createEmptyObject(parentTransform);
				armatureComponent.isUGUI=true;
				if(armatureComponent.GetComponentInParent<Canvas>()==null){
					var canvas = GameObject.Find("/Canvas");
					if(canvas){
						armatureComponent.transform.SetParent(canvas.transform);
					}
				}
				armatureComponent.transform.localScale = Vector2.one*100f;
				armatureComponent.transform.localPosition = Vector3.zero;
				var dragonBonesJSON = AssetDatabase.LoadMainAssetAtPath(dragonBonesJSONPath) as TextAsset;

				_changeDragonBonesData(armatureComponent, dragonBonesJSON);
			}
		}


		[MenuItem("Assets/Create/DragonBones/Create Unity Data", true)]
		private static bool _createUnityDataValidateMenuItem()
		{
			return _getDragonBonesJSONPaths(true).Count > 0;
		}

		[MenuItem("Assets/Create/DragonBones/Create Unity Data", false, 32)]
		private static void _createUnityDataMenuItem()
		{
			foreach (var dragonBonesJSONPath in _getDragonBonesJSONPaths(true))
			{
				var dragonBonesJSON = AssetDatabase.LoadMainAssetAtPath(dragonBonesJSONPath) as TextAsset;
				var textureAtlasJSONs = new List<string>();
				_getTextureAtlasConfigs(textureAtlasJSONs, AssetDatabase.GetAssetPath(dragonBonesJSON.GetInstanceID()));
				UnityDragonBonesData.TextureAtlas[] textureAtlas = new UnityDragonBonesData.TextureAtlas[textureAtlasJSONs.Count];
				for(int i=0;i<textureAtlasJSONs.Count;++i){
					string path = textureAtlasJSONs[i];
					//load textureAtlas data
					UnityDragonBonesData.TextureAtlas ta = new UnityDragonBonesData.TextureAtlas();
					ta.textureAtlasJSON = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
					//load texture
					path = path.Substring(0,path.LastIndexOf(".json"));
					ta.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path+".png");
					//load material
					ta.material = AssetDatabase.LoadAssetAtPath<Material>(path+"_Mat.mat");
					ta.uiMaterial = AssetDatabase.LoadAssetAtPath<Material>(path+"_UI_Mat.mat");
					textureAtlas[i] = ta;
				}
				CreateUnityDragonBonesData(dragonBonesJSON,textureAtlas);
			}
		}



		private static List<string> _getDragonBonesJSONPaths( bool isCreateUnityData = false)
        {
            var dragonBonesJSONPaths = new List<string>();
            foreach (var guid in Selection.assetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".json"))
                {
                    var jsonCode = File.ReadAllText(assetPath);
                    if (jsonCode.IndexOf("\"armature\":") > 0)
                    {
                        dragonBonesJSONPaths.Add(assetPath);
                    }
                }
				else if(!isCreateUnityData && assetPath.EndsWith("_Data.asset"))
				{
					UnityDragonBonesData data = AssetDatabase.LoadAssetAtPath<UnityDragonBonesData>(assetPath);
					if(data.dragonBonesJSON!=null){
						dragonBonesJSONPaths.Add(AssetDatabase.GetAssetPath(data.dragonBonesJSON));
					}
				}
            }

            return dragonBonesJSONPaths;
        }
        #endregion

        private static UnityArmatureComponent _createEmptyObject(UnityEngine.Transform parentTransform)
        {
            var gameObject = new GameObject("New Armature Object", typeof(UnityArmatureComponent));
            var armatureComponent = gameObject.GetComponent<UnityArmatureComponent>();
			gameObject.transform.SetParent(parentTransform, false);

            //
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = gameObject;
            EditorGUIUtility.PingObject(Selection.activeObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Armature Object");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            return armatureComponent;
        }

        private static void _getTextureAtlasConfigs(List<string> textureAtlasFiles, string filePath, string rawName = null, string suffix = "tex")
        {
            var folder = Directory.GetParent(filePath).ToString();
            var name = rawName != null ? rawName : filePath.Substring(0, filePath.LastIndexOf(".")).Substring(filePath.LastIndexOf("/") + 1);
			if(name.LastIndexOf("_ske")==name.Length-4){
				name = name.Substring(0,name.LastIndexOf("_ske"));
			}
            int index = 0;
            var textureAtlasName = "";
            var textureAtlasConfigFile = "";

            textureAtlasName = !string.IsNullOrEmpty(name) ? name + (!string.IsNullOrEmpty(suffix) ? "_" + suffix : suffix) : suffix;
            textureAtlasConfigFile = folder + "/" + textureAtlasName + ".json";

            if (File.Exists(textureAtlasConfigFile))
            {
                textureAtlasFiles.Add(textureAtlasConfigFile);
                return;
            }

            if (textureAtlasFiles.Count > 0 || rawName != null)
            {
                return;
            }

            while (true)
            {
                textureAtlasName = (!string.IsNullOrEmpty(name) ? name + (!string.IsNullOrEmpty(suffix) ? "_" + suffix : suffix) : suffix) + "_" + (index++);
                textureAtlasConfigFile = folder + "/" + textureAtlasName + ".json";
                if (File.Exists(textureAtlasConfigFile))
                {
                    textureAtlasFiles.Add(textureAtlasConfigFile);
                }
                else if (index > 1)
                {
                    break;
                }
            }

            _getTextureAtlasConfigs(textureAtlasFiles, filePath, "", suffix);
            if (textureAtlasFiles.Count > 0)
            {
                return;
            }

            index = name.LastIndexOf("_");
            if (index >= 0)
            {
                name = name.Substring(0, index);

                _getTextureAtlasConfigs(textureAtlasFiles, filePath, name, suffix);
                if (textureAtlasFiles.Count > 0)
                {
                    return;
                }

                _getTextureAtlasConfigs(textureAtlasFiles, filePath, name, "");
                if (textureAtlasFiles.Count > 0)
                {
                    return;
                }
            }

            if (suffix != "texture")
            {
                _getTextureAtlasConfigs(textureAtlasFiles, filePath, null, "texture");
            }
        }

        private static bool _changeDragonBonesData(UnityArmatureComponent _armatureComponent, TextAsset dragonBoneJSON)
        {
            if (dragonBoneJSON != null)
            {
                var textureAtlasJSONs = new List<string>();
                _getTextureAtlasConfigs(textureAtlasJSONs, AssetDatabase.GetAssetPath(dragonBoneJSON.GetInstanceID()));
				UnityDragonBonesData.TextureAtlas[] textureAtlas = new UnityDragonBonesData.TextureAtlas[textureAtlasJSONs.Count];
				for(int i=0;i<textureAtlasJSONs.Count;++i){
					string path = textureAtlasJSONs[i];
					//load textureAtlas data
					UnityDragonBonesData.TextureAtlas ta = new UnityDragonBonesData.TextureAtlas();
					ta.textureAtlasJSON = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
					//load texture
					path = path.Substring(0,path.LastIndexOf(".json"));
					ta.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path+".png");
					//load material
					ta.material = AssetDatabase.LoadAssetAtPath<Material>(path+"_Mat.mat");
					ta.uiMaterial = AssetDatabase.LoadAssetAtPath<Material>(path+"_UI_Mat.mat");
					textureAtlas[i] = ta;
				}
				UnityDragonBonesData data = CreateUnityDragonBonesData(dragonBoneJSON,textureAtlas);
				_armatureComponent.unityData = data;

				var dragonBonesData = _armatureComponent.LoadData(dragonBoneJSON, textureAtlas,data.dataName);
                if (dragonBonesData != null)
                {
                    Undo.RecordObject(_armatureComponent, "Set DragonBones");

					_armatureComponent.unityData = data;

                    var armatureName = dragonBonesData.armatureNames[0];
                    _changeArmatureData(_armatureComponent, armatureName, dragonBonesData.name);

                    _armatureComponent.gameObject.name = armatureName;

                    EditorUtility.SetDirty(_armatureComponent);

                    return true;
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Could not load dragonBones data.", "OK", null);

                    return false;
                }
            }
			else if (_armatureComponent.unityData != null)
            {
                Undo.RecordObject(_armatureComponent, "Set DragonBones");

				_armatureComponent.unityData = null;

                if (_armatureComponent.armature != null)
                {
                    _armatureComponent.Dispose(false);
                }

                EditorUtility.SetDirty(_armatureComponent);

                return true;
            }

            return false;
        }

		private static UnityDragonBonesData CreateUnityDragonBonesData(TextAsset dragonBonesJSON,UnityDragonBonesData.TextureAtlas[] textureAtlas){
			if(dragonBonesJSON){
				string path = AssetDatabase.GetAssetPath(dragonBonesJSON);
				path = path.Substring(0,path.Length-5);
				int index = path.LastIndexOf("_ske");
				if(index>0){
					path = path.Substring(0,index);
				}
				string dataPath = path+"_Data.asset";
				UnityDragonBonesData data = AssetDatabase.LoadAssetAtPath<UnityDragonBonesData>(dataPath);
				if(data==null){
					data = UnityDragonBonesData.CreateInstance<UnityDragonBonesData>();
					AssetDatabase.CreateAsset(data,dataPath);
				}
				string name = path.Substring(path.LastIndexOf("/")+1);
				data.dataName = name;
				data.dragonBonesJSON = dragonBonesJSON;

				if(textureAtlas!=null && textureAtlas.Length>0 && textureAtlas[0]!=null && textureAtlas[0].texture!=null){
					data.textureAtlas = textureAtlas;
				}
				AssetDatabase.SaveAssets();
				return data;
			}
			return null;
		}

        private static void _changeArmatureData(UnityArmatureComponent _armatureComponent, string armatureName, string dragonBonesName)
        {
            Slot slot = null;
            if (_armatureComponent.armature != null)
            {
                slot = _armatureComponent.armature.parent;
                _armatureComponent.Dispose(false);
            }
            _armatureComponent.armatureName = armatureName;

			_armatureComponent = UnityFactory.factory.BuildArmatureComponent(_armatureComponent.armatureName, dragonBonesName, null, _armatureComponent.unityData.dataName, _armatureComponent.gameObject,_armatureComponent.isUGUI);
            if (slot != null)
            {
                slot.childArmature = _armatureComponent.armature;
            }

            _armatureComponent.sortingLayerName = _armatureComponent.sortingLayerName;
            _armatureComponent.sortingOrder = _armatureComponent.sortingOrder;
        }

        private static void _clearGameObjectChildren(GameObject gameObject)
        {
            var children = new List<GameObject>();
            int childCount = gameObject.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                var child = gameObject.transform.GetChild(i).gameObject;
                children.Add(child);
            }

            for (int i = 0; i < childCount; ++i)
            {
                var child = children[i];
#if UNITY_EDITOR
                Object.DestroyImmediate(child);
#else
                Object.Destroy(child);
#endif
            }
        }

        private static List<string> _getSortingLayerNames()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

            return new List<string>(sortingLayersProperty.GetValue(null, new object[0]) as string[]);
        }

        private static UnityEngine.Transform _getSelectionParentTransform()
        {
            var parent = Selection.activeObject as GameObject;
            return parent != null ? parent.transform : null;
        }

        private int _armatureIndex = -1;
        private int _animationIndex = -1;
        private int _sortingLayerIndex = -1;
        private long _nowTime = 0;
        private List<string> _armatureNames = null;
        private List<string> _animationNames = null;
        private List<string> _sortingLayerNames = null;
        private UnityArmatureComponent _armatureComponent = null;

        void OnEnable()
        {
            _armatureComponent = target as UnityArmatureComponent;

            // 
            _nowTime = System.DateTime.Now.Ticks;
            _sortingLayerNames = _getSortingLayerNames();
            _sortingLayerIndex = _sortingLayerNames.IndexOf(_armatureComponent.sortingLayerName);

            // Update armature.
            if (
                !EditorApplication.isPlayingOrWillChangePlaymode &&
                _armatureComponent.armature == null &&
				_armatureComponent.unityData != null &&
                !string.IsNullOrEmpty(_armatureComponent.armatureName)
            )
            {
				//clear cache
				UnityFactory.factory.Clear(true);
                // Load data.
				var dragonBonesData = _armatureComponent.LoadData(_armatureComponent.unityData.dragonBonesJSON, _armatureComponent.unityData.textureAtlas,_armatureComponent.unityData.dataName);

                // Refresh texture atlas.
				UnityFactory.factory.RefreshAllTextureAtlas(_armatureComponent);

                // Refresh armature.
                _changeArmatureData(_armatureComponent, _armatureComponent.armatureName, dragonBonesData.name);

                // Refresh texture.
                _armatureComponent.armature.InvalidUpdate(null, true);

                // Play animation.
                if (!string.IsNullOrEmpty(_armatureComponent.animationName))
                {
                    _armatureComponent.animation.Play(_armatureComponent.animationName);
                }
				_armatureComponent.CollectBones();
            }

            // Update hideFlags.
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                _armatureComponent.armature != null &&
                _armatureComponent.armature.parent != null
            )
            {
                _armatureComponent.gameObject.hideFlags = HideFlags.NotEditable;
            }
            else
            {
                _armatureComponent.gameObject.hideFlags = HideFlags.None;
            }

            // 
            _updateParameters();
        }

        public override void OnInspectorGUI()
        {
            // DragonBones Data
            EditorGUILayout.BeginHorizontal();

			_armatureComponent.unityData = EditorGUILayout.ObjectField("DragonBones Data", _armatureComponent.unityData, typeof(UnityDragonBonesData), false) as UnityDragonBonesData;

            var created = false;
			if (_armatureComponent.unityData != null)
            {
                if (_armatureComponent.armature == null)
                {
                    if (GUILayout.Button("Create"))
                    {
                        created = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Reload"))
                    {
						if(EditorUtility.DisplayDialog("DragonBones Alert", "Are you sure you want to reload data", "Yes", "No")){
							created = true;
						}
                    }
                }
            }

            if (created)
			{	
				//clear cache
				UnityFactory.factory.Clear(true);
				if (_changeDragonBonesData(_armatureComponent, _armatureComponent.unityData.dragonBonesJSON))
                {
					_armatureComponent.CollectBones();
                    _updateParameters();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (_armatureComponent.armature != null)
            {
                var dragonBonesData = _armatureComponent.armature.armatureData.parent;

                // Armature
                if (UnityFactory.factory.GetAllDragonBonesData().ContainsValue(dragonBonesData) && _armatureNames != null)
                {
                    var armatureIndex = EditorGUILayout.Popup("Armature", _armatureIndex, _armatureNames.ToArray());
                    if (_armatureIndex != armatureIndex)
                    {
                        _armatureIndex = armatureIndex;
                        //_clearGameObjectChildren(_armatureComponent.gameObject);

                        var armatureName = _armatureNames[_armatureIndex];
                        _changeArmatureData(_armatureComponent, armatureName, dragonBonesData.name);
                        _updateParameters();
						if(_armatureComponent.bonesRoot!=null && _armatureComponent.unityBones!=null){
							_armatureComponent.ShowBones();
						}

                        _armatureComponent.gameObject.name = armatureName;
						_armatureComponent.zorderIsDirty = true;

                        EditorUtility.SetDirty(_armatureComponent);
						if (!Application.isPlaying && !_isPrefab()){
							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
                    }
                }

                // Animation
                if (_animationNames != null && _animationNames.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
					List<string> anims=new List<string>(_animationNames);
					anims.Insert(0,"<None>");
					var animationIndex = EditorGUILayout.Popup("Animation", _animationIndex+1, anims.ToArray())-1;
                    if (animationIndex != _animationIndex)
                    {
                        _animationIndex = animationIndex;
						if(animationIndex>=0){
	                        _armatureComponent.animationName = _animationNames[animationIndex];
							_armatureComponent.animation.Play(_armatureComponent.animationName);
							_updateParameters();
						}else{
							_armatureComponent.animationName = null;
							_armatureComponent.animation.Stop();
						}
						EditorUtility.SetDirty(_armatureComponent);
						if (!Application.isPlaying && !_isPrefab()){
							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
                    }

                    if (_animationIndex >= 0)
                    {
                        if (_armatureComponent.animation.isPlaying)
                        {
                            if (GUILayout.Button("Stop"))
                            {
                                _armatureComponent.animation.Stop();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Play"))
                            {
                                _armatureComponent.animation.Play();
                            }
                        }
                    }
					EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();

				if(!_armatureComponent.isUGUI){
					// Sorting Layer
					_sortingLayerIndex = EditorGUILayout.Popup("Sorting Layer", _sortingLayerIndex, _sortingLayerNames.ToArray());
					if (_sortingLayerNames[_sortingLayerIndex] != _armatureComponent.sortingLayerName)
					{
						Undo.RecordObject(_armatureComponent, "Sorting Layer");
						_armatureComponent.sortingLayerName = _sortingLayerNames[_sortingLayerIndex];
						EditorUtility.SetDirty(_armatureComponent);
						if (!Application.isPlaying && !_isPrefab()){
							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
					}

					// Sorting Order
					var sortingOrder = EditorGUILayout.IntField("Order in Layer", _armatureComponent.sortingOrder);
					if (sortingOrder != _armatureComponent.sortingOrder)
					{
						Undo.RecordObject(_armatureComponent, "Edit Sorting Order");
						_armatureComponent.sortingOrder = sortingOrder;
						EditorUtility.SetDirty(_armatureComponent);
						if (!Application.isPlaying && !_isPrefab()){
							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
					}

					// ZSpace
					EditorGUILayout.BeginHorizontal();
					_armatureComponent.zSpace = EditorGUILayout.Slider("Z Space", _armatureComponent.zSpace, 0.0f, 0.2f);
					EditorGUILayout.EndHorizontal();
				}

                // TimeScale
                EditorGUILayout.BeginHorizontal();
                _armatureComponent.animation.timeScale = EditorGUILayout.Slider("Time Scale", _armatureComponent.animation.timeScale, 0.0f, 2.0f);
                EditorGUILayout.EndHorizontal();

                // Flip
                EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Flip");
				bool flipX = _armatureComponent.flipX;
				bool flipY = _armatureComponent.flipY;
				_armatureComponent.flipX = GUILayout.Toggle(_armatureComponent.flipX, "X",GUILayout.Width(30));
				_armatureComponent.flipY = GUILayout.Toggle(_armatureComponent.flipY, "Y",GUILayout.Width(30));
				_armatureComponent.armature.flipX = _armatureComponent.flipX;
				_armatureComponent.armature.flipY = _armatureComponent.flipY;
				if(_armatureComponent.flipX!=flipX || _armatureComponent.flipY!=flipY){
					EditorUtility.SetDirty(_armatureComponent);
					if (!Application.isPlaying && !_isPrefab()){
						EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
					}
				}
                EditorGUILayout.EndHorizontal();

				//normals
				EditorGUILayout.BeginHorizontal();
				_armatureComponent.addNormal = EditorGUILayout.Toggle("Normals", _armatureComponent.addNormal);
				EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

			if(_armatureComponent.armature!=null && _armatureComponent.armature.parent==null)
			{
				if(_armatureComponent.unityBones!=null && _armatureComponent.bonesRoot!=null)
				{
					_armatureComponent.boneHierarchy = EditorGUILayout.Toggle("Bone Hierarchy", _armatureComponent.boneHierarchy);
				}
				EditorGUILayout.BeginHorizontal();
				if(!Application.isPlaying){
					if(_armatureComponent.unityBones!=null && _armatureComponent.bonesRoot!=null){
						if(GUILayout.Button("Remove Bones",GUILayout.Height(20))){
							if(EditorUtility.DisplayDialog("DragonBones Alert", "Are you sure you want to remove bones", "Yes", "No")){
								_armatureComponent.RemoveBones();
							}
						}
					}else{
						if(GUILayout.Button("Show Bones",GUILayout.Height(20))){
							_armatureComponent.ShowBones();
						}
					}
				}
				if(!Application.isPlaying && !_armatureComponent.isUGUI){
					UnityCombineMesh ucm = _armatureComponent.gameObject.GetComponent<UnityCombineMesh>();
					if(!ucm) {
						if(GUILayout.Button("Add Mesh Combine",GUILayout.Height(20))){
							ucm = _armatureComponent.gameObject.AddComponent<UnityCombineMesh>();
						}
					}
				}
				EditorGUILayout.EndHorizontal();
			}

            if (!EditorApplication.isPlayingOrWillChangePlaymode && Selection.activeObject == _armatureComponent.gameObject)
            {
                EditorUtility.SetDirty(_armatureComponent);
                HandleUtility.Repaint();
            }
        }

        void OnSceneGUI()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && _armatureComponent.armature != null)
            {
                var dt = System.DateTime.Now.Ticks - _nowTime;
                if (dt >= 1.0f / _armatureComponent.armature.armatureData.frameRate * 1000000.0f)
                {
                    _armatureComponent.armature.AdvanceTime(dt * 0.0000001f);
                    _nowTime = System.DateTime.Now.Ticks;
                }
            }
        }

        private void _updateParameters()
        {
            if (_armatureComponent.armature != null)
            {
				if(_armatureComponent.armature.armatureData.parent!=null){
	                _armatureNames = _armatureComponent.armature.armatureData.parent.armatureNames;
	                _animationNames = _armatureComponent.armature.armatureData.animationNames;
	                _armatureIndex = _armatureNames.IndexOf(_armatureComponent.armature.name);
					if(!string.IsNullOrEmpty(_armatureComponent.animationName)){
						_animationIndex = _animationNames.IndexOf(_armatureComponent.animationName);
					}
				}else
				{
					_armatureNames = null;
					_animationNames = null;
					_armatureIndex = -1;
					_animationIndex = -1;
				}
            }
            else
            {
                _armatureNames = null;
                _animationNames = null;
                _armatureIndex = -1;
                _animationIndex = -1;
            }
        }

		private bool _isPrefab(){
			return PrefabUtility.GetPrefabParent(_armatureComponent.gameObject) == null 
				&& PrefabUtility.GetPrefabObject(_armatureComponent.gameObject) != null;
		}
    }
}
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Ultramonster
{

    public class FishManager : MonoBehaviour
    {

        #region Variables
        public static FishManager instance;
        [SerializeField] private GameObject splashScreenGb;
        [SerializeField] private UnityEngine.UI.Image downloadBar;

        [SerializeField] private GameObject mainCanvasGb;

        public UnityEngine.UI.Text playerScoreText;
        public UnityEngine.UI.Text downloadProgressText;
        public Canon canonObject;

        [Header("Paths"), Space(10)]
        public PathCreator[] pathCreatorLToR;
        public PathCreator[] pathCreatorRToL;

        public PathCreator[] lessCurvePathLtoR;
        public PathCreator[] lessCurvePathRtoL;
        public PathCreator[] straightPathRtoL;
        public PathCreator[] straightPathLtoR;

        public PathCreator[] groundPathLToR;
        
        public PathCreator[] groundPathRToL;

        public PathCreator[] dragonPathRToL;
        public PathCreator[] dragonPathLToR;

        [Space(10)]
        [SerializeField] int[] probabilitiesvalue;  //tropical fish = 50 , group = 10 , shark = 10 , mermaid = 10 , turtle = 15 , dragon = 5
                                                    

        [SerializeField] List<float> cummulativeProbab = new List<float>();

        [SerializeField] int totalFishes;
        [SerializeField] int levelNumber;
        int currentLoadedFishesAmount = 0;

        [SerializeField] List<LevelDetails> specialCharacterDisableList = new List<LevelDetails>();

        public GameObject deadEffect;
        private float assetsDownloadProgrerss;
        public List<AssetReference> totalReferences = new List<AssetReference>();
        #endregion

        private void Start()
        {
            instance = this;
            CreateCummulativeProbability();
            LoadAddressable();
            //Invoke("StartGame" , 2);
        }

        void StartGame()
        {
            splashScreenGb.SetActive(false);
            mainCanvasGb.SetActive(true);
            GameManager.Instance.gameStarted = true;
            GameManager.Instance.gameEnded = false;
            InvokeRepeating("Probability", 0, 0.5f);
        }


        #region Fish Pooling Using Addressables

        #region Pooling Variables
        /*[Header("Fishes Pool Amount"), Space(10)]
        [SerializeField] int mermaidPoolAmount;
        [SerializeField] int sharkPoolAmount;
        [SerializeField] int turtlePoolAmount;
        [SerializeField] int groupPoolAmount;
        [SerializeField] int dragonPoolAmount;
        [SerializeField] int tropicalFishPoolAmount;
        [SerializeField] int crabPoolAmount;
        [SerializeField] int bugsPoolAmout;


        [Header("Fishes References"), Space(10)]
        [SerializeField] AssetReferenceGameObject[] sharkReferences;
        [SerializeField] AssetReferenceGameObject[] mermaidReferences;
        [SerializeField] AssetReferenceGameObject[] groupReferences;
        [SerializeField] AssetReferenceGameObject[] tropicalFishReferences;

        [SerializeField] AssetReferenceGameObject turtleReference;
        [SerializeField] AssetReferenceGameObject dragonReference;

        [SerializeField] AssetReferenceGameObject[] bugReferences;
        [SerializeField] AssetReferenceGameObject[] crabReferences;


        [Header("Fishes Pool"), Space(10)]
        [HideInInspector] public List<GameObject> mermaidPool = new List<GameObject>();
        [HideInInspector] public List<GameObject> sharkPool = new List<GameObject>();
        [HideInInspector] public List<GameObject> turtlePool = new List<GameObject>();
        [HideInInspector] public List<GameObject> groupPool = new List<GameObject>();
        [HideInInspector] public List<GameObject> dragonPool = new List<GameObject>();
        [HideInInspector] public List<GameObject> tropicalFishPool = new List<GameObject>();
        [HideInInspector] public List<GameObject> crabPool = new List<GameObject>();
        [HideInInspector] public List<GameObject> bugPool = new List<GameObject>();*/
        #endregion

        #region Pooling Variables new

        [Header("Level Details") , Space(10)]
        [SerializeField] List<LevelDetails> levelDetails;
        #endregion

        #region Fish Pooling Methods

        /// <summary>
        /// Load assets from addressable
        /// </summary>
        void LoadAddressable()
        {
            StartCoroutine(InitAddressable());
        }

        IEnumerator InitAddressable()
        {
            Debug.Log("Initializing addressable............");
            AsyncOperationHandle<IResourceLocator> handle = Addressables.InitializeAsync();
            yield return handle;
            Debug.Log("Handle returned............");
            if (handle.IsDone)
            {
                Debug.Log("Handled properly. Now creating pool............");
                Addressables.InitializeAsync().Completed += CreatePoolCallback;
                Debug.Log("No error while creating pool");
            }
        }


        /// <summary>
        /// Addressable callback
        /// </summary>
        /// <param name="obj"></param>
        private void CreatePoolCallback(AsyncOperationHandle<IResourceLocator> obj)
        {
            StartCoroutine(StartGameOnLoadingCompleted());
            StartCoroutine(ProgressHandler());
            foreach(LevelDetails details in levelDetails)
            {
                StartCoroutine(CreateFishPool(details.characterPool, details.poolAmount, details.characterReferences , details.characterReference, details.disabledCharacterPool));
            }
        }



        /// <summary>
        /// Create fish pool from addressabls
        /// </summary>
        /// <param name="poolList"></param>
        /// <param name="poolAmount"></param>
        /// <param name="references"></param>
        /// <param name="reference"></param>
        IEnumerator CreateFishPool(List<GameObject> poolList, int poolAmount, AssetReferenceGameObject[] references, AssetReferenceGameObject reference,
            List<GameObject> disabledList)
        {
            yield return null;

            if (references.Length > 0)
            {
                foreach (AssetReferenceGameObject argb in references)
                {
                    totalReferences.Add(argb);
                    //var load = argb.LoadAssetAsync<GameObject>();
                    argb.LoadAssetAsync<GameObject>().Completed += (init) =>
                    {
                        currentLoadedFishesAmount += 1;
                        //Debug.Log(currentLoadedFishesAmount);
                        downloadBar.fillAmount = currentLoadedFishesAmount / totalFishes;
                        //Debug.Log("###### " + init.Result + " ######");
                        for (int j = 0; j < poolAmount; j++)
                        {
                            GameObject fish = Instantiate(init.Result, transform);
                            fish.SetActive(false);
                            if (fish.GetComponent<FishController>().characterType == FishController.CharacterType.Normal)
                                poolList.Add(fish);
                            else
                                disabledList.Add(fish);
                        }

                        if(disabledList.Count > 0)
                        {
                            //Debug.Log("********* Disable list 1**********");
                            StartCoroutine(SpecialCharacterDisableList(poolList, disabledList));
                        }
                    };

                }
            }
            else if (references.Length == 0)
            {
                reference.LoadAssetAsync<GameObject>().Completed += (init) =>
                {
                    currentLoadedFishesAmount += 1;
                    downloadBar.fillAmount = currentLoadedFishesAmount / totalFishes;
                    
                    for (int i = 0; i < poolAmount; i++)
                    {
                        GameObject fish = Instantiate(init.Result, transform);
                        fish.SetActive(false);
                        if (fish.GetComponent<FishController>().characterType == FishController.CharacterType.Normal)
                            poolList.Add(fish);
                        else
                            disabledList.Add(fish);
                    }

                    if(disabledList.Count > 0)
                    {
                        //Debug.Log("********* Disable list 2**********");
                        StartCoroutine(SpecialCharacterDisableList(poolList, disabledList));
                    }
                };

                totalReferences.Add(reference);
            }
        }

        IEnumerator SpecialCharacterDisableList(List<GameObject> pool , List<GameObject> disableList)
        {
            yield return new WaitForSeconds(disableList[0].GetComponent<FishController>().respawnTimeForSpecialCharacter);
            pool.AddRange(disableList);
            disableList.Clear();
        }



        IEnumerator ProgressHandler()
        {
            Debug.Log("Start calculating progress");
            yield return new WaitUntil(() => totalReferences.Count == totalFishes);

            assetsDownloadProgrerss = 0;

            while(assetsDownloadProgrerss < 1)
            {
                foreach(AssetReference argb in totalReferences)
                {
                    assetsDownloadProgrerss += argb.OperationHandle.PercentComplete;
                }

                assetsDownloadProgrerss = assetsDownloadProgrerss / totalReferences.Count;
                downloadBar.fillAmount = assetsDownloadProgrerss;
                if (assetsDownloadProgrerss > 1)
                    assetsDownloadProgrerss = 1;
                downloadProgressText.text = "Loading " + (assetsDownloadProgrerss * 100).ToString("F1") + "%";
                //Debug.Log("progress " + assetsDownloadProgrerss);
                yield return null;
            }
        }


        IEnumerator StartGameOnLoadingCompleted()
        {
            Debug.Log("Checking");
            //Debug.Log(currentLoadedFishesAmount)
            yield return new WaitUntil(() => currentLoadedFishesAmount == totalFishes);

            StartGame();

            Debug.Log("All assets loaded");
        }


        /// <summary>
        /// Get fish from fish pool (from poolList)
        /// </summary>
        /// <param name="poolList"></param>
        /// <returns></returns>
        GameObject GetFish(List<GameObject> poolList)
        {
            if(poolList.Count > 0)
            {
                GameObject fish = poolList[Random.Range(0, poolList.Count)];
                //foreach(GameObject gb in poolList)
                //{
                //    if (!gb.activeInHierarchy)
                //        return gb;
                //}

                if (!fish.activeInHierarchy)
                {
                    return fish;
                }
                else
                {
                    //GetFish(poolList);

                    foreach (GameObject gb in poolList)
                    {
                        if (!gb.activeInHierarchy)
                            return gb;
                    }
                }
            }
            
            return null;
        }


        /// <summary>
        /// Put Back object in pool
        /// </summary>
        /// <param name="_fish"></param>
        public void PutBackToPool(GameObject _fish)
        {
            _fish.SetActive(false);
            _fish.transform.localPosition = Vector3.zero;

            if (_fish.GetComponent<FishController>().locked)
            {
                _fish.GetComponent<FishController>().locked = false;
                canonObject.lockedTargetGb = null;
                canonObject.isTargetLocked = false;
            }
        }

        #endregion

        #endregion

        #region Probability

        /// <summary>
        /// Calculate cummulative probability of fishes
        /// </summary>
        void CreateCummulativeProbability()
        {
            float c = 0;
            //for (int i = 0; i < probabilitiesvalue.Length; i++)
            //{
            //    c += probabilitiesvalue[i];
            //    cummulativeProbab.Add(c);
            //}

            foreach(LevelDetails details in levelDetails)
            {
                c += details.probability;
                cummulativeProbab.Add(c);
            }

            Debug.Log("Probability calculated.....");
        }


        /// <summary>
        /// Spawn fishes according to probability
        /// Cummulative probability is used 
        /// </summary>
        void Probability()
        {
            int random = Random.Range(0, (int)cummulativeProbab[cummulativeProbab.Count - 1] + 1);
            //Debug.Log(random);
            Spawn(random);
        }

        void Spawn(int random)
        {

            GameObject currentCharacter = null;
            int characterIndex = 0;

            if (random <= cummulativeProbab[0])
            {
                //SpawnFish(tropicalFishPool);
                 characterIndex = 0;
                 currentCharacter = SpawnFish(levelDetails[0].characterPool);
            }
            else if (random <= cummulativeProbab[1] && random > cummulativeProbab[0])
            {
                characterIndex = 1;
                //SpawnFish(groupPool);
                currentCharacter = SpawnFish(levelDetails[1].characterPool);
            }
            else if (random <= cummulativeProbab[2] && random > cummulativeProbab[1])
            {
                //SpawnFish(sharkPool);
                characterIndex = 2;
                currentCharacter = SpawnFish(levelDetails[2].characterPool);
            }
            else if (random <= cummulativeProbab[3] && random > cummulativeProbab[2])
            {
                //SpawnFish(mermaidPool);
                characterIndex = 3;
                currentCharacter = SpawnFish(levelDetails[3].characterPool);
            }
            else if (random <= cummulativeProbab[4] && random > cummulativeProbab[3])
            {
                //SpawnFish(turtlePool);
                characterIndex = 4;
                currentCharacter = SpawnFish(levelDetails[4].characterPool);
            }
            else if (random <= cummulativeProbab[5] && random > cummulativeProbab[4])
            {
                //SpawnFish(dragonPool);
                characterIndex = 5;
                currentCharacter = SpawnFish(levelDetails[5].characterPool);
            }
            else if(random <= cummulativeProbab[6] && random > cummulativeProbab[5])
            {
                characterIndex = 6;
                currentCharacter = SpawnFish(levelDetails[6].characterPool);
            }
            else if (random <= cummulativeProbab[7] && random > cummulativeProbab[6])
            {
                characterIndex = 7;
                currentCharacter = SpawnFish(levelDetails[7].characterPool);
            }
            else if (random <= cummulativeProbab[8] && random > cummulativeProbab[7])
            {
                characterIndex = 8;
                currentCharacter = SpawnFish(levelDetails[8].characterPool);
            }
            else if (random <= cummulativeProbab[9] && random > cummulativeProbab[8])
            {
                characterIndex = 9;
                currentCharacter = SpawnFish(levelDetails[9].characterPool);
            }
            else if (random <= cummulativeProbab[10] && random > cummulativeProbab[9])
            {
                characterIndex = 10;
                currentCharacter = SpawnFish(levelDetails[10].characterPool);
            }
            else if (random <= cummulativeProbab[11] && random > cummulativeProbab[10])
            {
                characterIndex = 11;
                currentCharacter = SpawnFish(levelDetails[11].characterPool);
            }
            else if (random <= cummulativeProbab[12] && random > cummulativeProbab[11])
            {
                characterIndex = 12;
                currentCharacter = SpawnFish(levelDetails[12].characterPool);
            }
            else if (random <= cummulativeProbab[13] && random > cummulativeProbab[12])
            {
                characterIndex = 13;
                currentCharacter = SpawnFish(levelDetails[13].characterPool);
            }


            if(currentCharacter == null)
            {
                characterIndex = 0;
                currentCharacter = SpawnFish(levelDetails[0].characterPool);
            }

            SpecialCharacterManage(currentCharacter , characterIndex);
        }


        /// <summary>
        /// Remove special character from pool for few seconds
        /// </summary>
        /// <param name="currentCharacter"></param>
        /// <param name="characterIndex"></param>
        void SpecialCharacterManage(GameObject currentCharacter , int characterIndex)
        {
            if(currentCharacter != null)
            {
                //if (currentCharacter.GetComponent<FishController>().characterType == FishController.CharacterType.Special)
                //{
                    //specialCharacterDisableList.Add(levelDetails[characterIndex]);
                    for (int i = 0; i < levelDetails[characterIndex].characterPool.Count; i++)
                    {
                        levelDetails[characterIndex].disabledCharacterPool.Add(levelDetails[characterIndex].characterPool[i]);
                    }

                    float time = levelDetails[characterIndex].characterPool[0].GetComponent<FishController>().respawnTimeForSpecialCharacter;
                    levelDetails[characterIndex].characterPool.Clear();
                    StartCoroutine(SwapDisabledCharacterPool(time, characterIndex));
                //}
            }
           
        }

        IEnumerator SwapDisabledCharacterPool(float time , int index)
        {
            yield return new WaitForSeconds(time);
            for(int i = 0; i < levelDetails[index].disabledCharacterPool.Count; i++)
            {
                levelDetails[index].characterPool.Add(levelDetails[index].disabledCharacterPool[i]);
            }

            levelDetails[index].disabledCharacterPool.Clear();
        }


        #endregion


        /// <summary>
        /// Spawn Fishes in the scene
        /// </summary>
        /// <param name="poolList"></param>
        public GameObject SpawnFish(List<GameObject> poolList)
        {

            GameObject fish = GetFish(poolList);


            if (fish != null)
            {

                fish.SetActive(true);

                int selectPath = Random.Range(0, 2);

                if(levelNumber == 1 || levelNumber == 2 || levelNumber == 3)
                {
                    switch (selectPath)
                    {
                        case 0:                        //for left to right paths
                            if (fish.tag == "Group")    //only for group fishes
                            {
                                var p1 = straightPathLtoR[Random.Range(0, straightPathLtoR.Length - 1)];
                                fish.GetComponent<FishController>().pathCreator = p1;
                                foreach (Transform child in fish.transform)
                                {
                                    child.GetChild(0).gameObject.SetActive(true);
                                    child.GetChild(0).GetComponent<FishController>().speed = 0;
                                    child.GetChild(0).GetChild(0).rotation = Quaternion.Euler(90, 90, 0);
                                }

                            }
                            else              //for other fishes
                            {
                                AdjustSizeAndLayer(fish);

                                if (fish.tag == "Shark" ||
                                    //fish.tag == "Mermaid" ||
                                    fish.tag == "Dragon" ||
                                    fish.tag == "Turtle" ||
                                    fish.tag == "Whale" ||
                                    fish.tag == "Seahorse" ||
                                    fish.tag == "Phoenix")
                                {
                                    var p1 = lessCurvePathLtoR[Random.Range(0, lessCurvePathLtoR.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p1;
                                }
                                else if (fish.tag == "Panda" ||
                                 fish.tag == "Wizard" ||
                                 fish.tag == "Crocodile" ||
                                 fish.tag == "Bull" ||
                                 fish.tag == "Crab")
                                {
                                    var p2 = groundPathLToR[Random.Range(0, groundPathLToR.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p2;
                                }
                                else if(fish.tag == "Mermaid")
                                {
                                    var p3 = straightPathLtoR[Random.Range(0, straightPathLtoR.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p3;
                                }
                                else if(fish.tag == "Dragon")
                                {
                                    var p3 = dragonPathLToR[Random.Range(0, dragonPathLToR.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p3;
                                }
                                else
                                {
                                    var p = pathCreatorLToR[Random.Range(0, pathCreatorLToR.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p;
                                }

                                
                                
                                if (fish.tag == "Shark"
                                    || fish.tag == "TropicalFish"
                                    || fish.tag == "RayFish"
                                    || fish.tag == "Dragon"
                                    || fish.tag == "Turtle"
                                    || fish.tag == "Panda" 
                                    || fish.tag == "Wizard"
                                    || fish.tag == "Whale" 
                                    || fish.tag == "Seahorse" 
                                    || fish.tag == "Phoenix"
                                    || fish.tag == "Crocodile"
                                    || fish.tag == "Bull"
                                    || fish.tag == "Crab"
                                    || fish.tag == "Octopus"
                                    || fish.tag == "AngularFish"
                                    )
                                {
                                    //fish.transform.GetChild(0).localRotation = Quaternion.Euler(0, 180, 180);
                                }
                                    //fish.transform.GetChild(0).localRotation = Quaternion.Euler(0, 180, 180);

                            }

                            break;
                        case 1:                             //for right to left path
                            if (fish.tag == "Group")        //for group fishes
                            {
                                var q1 = straightPathRtoL[Random.Range(0, straightPathRtoL.Length - 1)];
                                fish.GetComponent<FishController>().pathCreator = q1;
                                foreach (Transform child in fish.transform)
                                {
                                    child.GetChild(0).gameObject.SetActive(true);
                                    child.GetChild(0).GetComponent<FishController>().speed = 0;
                                    child.GetChild(0).GetChild(0).rotation = Quaternion.Euler(-90, -90,0);
                                }
                            }
                            else                            //for other fishes
                            {
                                AdjustSizeAndLayer(fish);
                                if (fish.tag == "Shark" ||
                                   //fish.tag == "Mermaid" ||
                                   fish.tag == "Dragon" ||
                                   fish.tag == "Turtle" ||
                                   fish.tag == "Whale" ||
                                   fish.tag == "Seahorse" ||
                                   fish.tag == "Phoenix")
                                {
                                    var p1 = lessCurvePathRtoL[Random.Range(0, lessCurvePathRtoL.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p1;
                                }
                                else if (fish.tag == "Panda" ||
                                   fish.tag == "Wizard" ||
                                   fish.tag == "Crocodile" ||
                                   fish.tag == "Bull" ||
                                   fish.tag == "Crab")
                                {
                                    var p2 = groundPathRToL[Random.Range(0, groundPathRToL.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p2;
                                }
                                else if (fish.tag == "Mermaid")
                                {
                                    var p3 = straightPathRtoL[Random.Range(0, straightPathRtoL.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p3;
                                }
                                else if (fish.tag == "Dragon")
                                {
                                    var p3 = dragonPathRToL[Random.Range(0, dragonPathRToL.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p3;
                                }

                                else
                                {
                                    var p = pathCreatorRToL[Random.Range(0, pathCreatorRToL.Length - 1)];
                                    fish.GetComponent<FishController>().pathCreator = p;
                                }

                                if (fish.tag == "Shark"
                                   || fish.tag == "TropicalFish"
                                   || fish.tag == "RayFish"
                                   || fish.tag == "Dragon"
                                   || fish.tag == "Turtle"
                                   || fish.tag == "Panda"
                                   || fish.tag == "Wizard"
                                   || fish.tag == "Whale"
                                   || fish.tag == "Seahorse"
                                   || fish.tag == "Phoenix"
                                   || fish.tag == "Crocodile"
                                   || fish.tag == "Bull"
                                   || fish.tag == "Crab"
                                   || fish.tag == "Octopus"
                                   || fish.tag == "AngularFish"
                                   )
                                {
                                    //fish.transform.GetChild(0).localRotation = Quaternion.Euler(180, 180, 180);
                                }
                                    
                            }

                            break;
                    }
                }
                else if (levelNumber == 4)
                {
                    switch(selectPath)
                    {
                        case 0:
                            if (fish.tag == "Ant" ||
                                fish.tag == "Caterpiller" ||
                                fish.tag == "Scorpion" ||
                                fish.tag == "Spider")
                            {
                                var q = lessCurvePathLtoR[Random.Range(0, lessCurvePathLtoR.Length - 1)];
                                fish.GetComponent<FishController>().pathCreator = q;
                                fish.transform.GetChild(0).localRotation = Quaternion.Euler(0, 180, 180);
                            }
                            else
                            {
                                var q = pathCreatorLToR[Random.Range(0, pathCreatorLToR.Length - 1)];
                                fish.GetComponent<FishController>().pathCreator = q;
                                fish.transform.GetChild(0).localRotation = Quaternion.Euler(0, 180, 180);
                            }
                            break;
                        case 1:
                            if (fish.tag == "Ant" ||
                                fish.tag == "Caterpiller" ||
                                fish.tag == "Scorpion" ||
                                fish.tag == "Spider")
                            {
                                var q = lessCurvePathRtoL[Random.Range(0, lessCurvePathRtoL.Length - 1)];
                                fish.GetComponent<FishController>().pathCreator = q;
                                fish.transform.GetChild(0).localRotation = Quaternion.Euler(180, 180, 180);
                            }
                            else
                            {
                                var q = pathCreatorRToL[Random.Range(0, pathCreatorRToL.Length - 1)];
                                fish.GetComponent<FishController>().pathCreator = q;
                                fish.transform.GetChild(0).localRotation = Quaternion.Euler(180, 180, 180);
                            }
                            break;
                    }
                }         
            }

            return fish;
        }


     

        void AdjustSizeAndLayer(GameObject fish)
        {

           /* int layerNo = Random.Range(-5, -11);

            fish.GetComponentInChildren<SpriteRenderer>().sortingOrder = layerNo;

            switch (layerNo)
            {
                case -5:
                    fish.transform.localScale = new Vector3(1f, 1f, 1f);
                    break;
                case -6:
                    fish.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                    break;
                case -7:
                    fish.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    break;
                case -8:
                    fish.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                    break;
                case -9:
                    fish.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    break;
                case -10:
                    fish.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
            }*/
        }

    }

}

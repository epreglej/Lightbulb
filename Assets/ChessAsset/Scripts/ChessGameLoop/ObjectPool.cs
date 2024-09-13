using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ChessMainLoop
{
    /// <summary>
    /// Contains pools for objects and methods to put objects and recieve objects to and from pools.
    /// </summary>
    public  class ObjectPool : MonoBehaviour
    {
        private Dictionary<string, Queue<GameObject>> _poolDictionary;
        [SerializeField]
        private List<GameObject> _prefabs;
        private Queue<Piece> _pieces;

        private static ObjectPool _instance;
        public static ObjectPool Instance { get => _instance; } 

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private void Start()
        {
            _poolDictionary = new Dictionary<string, Queue<GameObject>>();
            _pieces = new Queue<Piece>();

            Queue<GameObject> queue;

            foreach (GameObject _prefab in _prefabs)
            {
                queue = new Queue<GameObject>();
                _poolDictionary.Add(_prefab.name, queue);
            }
        }

        /// <summary>
        /// Returns number of path objects indexed by name equal to quantity parameter. Gets objects from pool or instantiates new ones if quantity in pool isnt enough.
        /// </summary>
        /// <returns>List of path objects quantity long</returns>
        public GameObject[] GetHighlightPaths(int _quantity, string _name)
        {
            GameObject[] _paths = new GameObject[_quantity];

            for(int i = 0; i < _quantity; i++)
            {
                if (_poolDictionary[_name].Count > 0)
                {
                    _paths[i] = _poolDictionary[_name].Dequeue();
                    _paths[i].SetActive(true);
                }
                else
                {
                    _paths[i]= Instantiate(_prefabs.Where(obj => obj.name == _name).SingleOrDefault(), transform.parent);
                }
            }

            return _paths;
        }

        /// <summary>
        /// Returns a singular path object indexed by name
        /// </summary>
        /// <returns>Path object of name</returns>
        public GameObject GetHighlightPath(string _name)
        {
            GameObject _path;
            if (_poolDictionary[_name].Count > 0)
            {
                _path=_poolDictionary[_name].Dequeue();
                _path.SetActive(true);
            }
            else
            {
                _path = Instantiate(_prefabs.Where(obj => obj.name == _name).SingleOrDefault(), transform.parent);            
            }

            return _path;
        }

        /// <summary>
        /// Disables a path object and poots it back into pool
        /// </summary>
        public void RemoveHighlightPath(PathPiece _path)
        {
            _poolDictionary[_path.Name].Enqueue(_path.gameObject);
            _path.gameObject.SetActive(false);
        }

        public void AddPiece(Piece _piece)
        {
            _pieces.Enqueue(_piece);
            _piece.gameObject.SetActive(false);
        }

        public void ResetPieces()
        {
            while (_pieces.Count > 0)
            {
                _pieces.Dequeue().gameObject.SetActive(true);
            }
        }
    }
}

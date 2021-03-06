using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Core
{
    public class NodeObject : SessionMember
    {
        public string value;
        public GameObject[] neighborNodes;
        public int[] costs;
        public GameObject edgeObject;
        public Node Node;
        private Renderer _nodeRenderer;
        private Ticker _ticker = Ticker.Instance;

        void Awake()
        {
            (Node, int)[] neighbors = new (Node, int)[neighborNodes.Length];
            Node = new Node(value!, neighbors!);

            TMP_Text text = GetComponentInChildren<TMP_Text>();
            text!.text = value!;

            _nodeRenderer = gameObject.GetComponent<Renderer>();
        }

        private void Start()
        {
            PopulateNode();
            DrawEdges();
        }

        protected override void ONSessionStateChanged(SessionState state)
        {
            switch (state)
            {
                case SessionState.Running:
                    Node!.ONStateChanged += SetColor;
                    PopulateNode();
                    break;
                case SessionState.NotRunning:
                    StopAllCoroutines();
                    _nodeRenderer.material.color = Color.white;
                    (Node, int)[] neighbors = new (Node, int)[neighborNodes.Length];
                    Node = new Node(value!, neighbors!);
                    Node!.ONStateChanged -= SetColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void PopulateNode()
        {
            try
            {
                for (int i = 0; i < neighborNodes.Length; i++)
                {
                    Node.Neighbors[i] = (neighborNodes[i].GetComponent<NodeObject>().Node, costs[i]);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void DrawEdges()
        {
            try
            {
                for (var i = 0; i < neighborNodes.Length; i++)
                {
                    var node = neighborNodes[i];
                    var cost = costs[i];

                    //Get parent transform and its position
                    Transform parentTransform = transform;
                    Vector3 position = parentTransform.position;

                    //Create edge at parent's position
                    GameObject edge = Instantiate(edgeObject, position, Quaternion.identity, parentTransform);

                    //Rotate edge to point at designated neighbor node
                    Vector3 nodeTransform = node.transform.position;
                    edge.transform.LookAt(nodeTransform);

                    //Shift edge to the right by 0.1 units
                    edge.transform.Translate(0.1f, 0, 0, Space.Self);

                    //Stretch edge to reach designated neighbor node
                    float edgeLength = Vector3.Distance(position, nodeTransform) - 0.95f;
                    SpriteRenderer edgeRenderer = edge.GetComponentInChildren<SpriteRenderer>();
                    edgeRenderer.size = new Vector2(edgeLength, edgeRenderer.size.y);

                    // Display Cost
                    TMP_Text costText = edge.GetComponentInChildren<TMP_Text>();
                    costText.text = cost.ToString();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void SetColor()
        {
            StartCoroutine(WaitThenSetColor());
            _ticker.Tick(1);
        }

        private IEnumerator WaitThenSetColor()
        {
            yield return new WaitForSeconds(_ticker.TimeInSeconds);
            _nodeRenderer.material.color = Color.magenta;
        }
    }
}
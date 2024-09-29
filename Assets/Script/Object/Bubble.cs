using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bubble : MonoBehaviour
{
    // 이동
    private float speed;
    private bool moveToTarget;
    private bool isMoving;

    // 특수 버블
    public bool moveToBox;
    public bool isConnected;

    // 점수
    private int dropPoint;

    public Image sprite; 
    private RectTransform rectTransform;
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private ValueManager.BubbleColor bcolor;

    private BubbleManager bubbleManager;
    private Timer timer;
    private GridCell gridPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = GetComponent<Timer>();
        rectTransform = GetComponent<RectTransform>();

        targetPosition = new Vector2(GameManager.Instance.ScoreGate.position.x, GameManager.Instance.ScoreGate.position.y);

        dropPoint = 5;
        speed = 5.0f;
        moveToTarget = false;
        moveToBox = false;
        isMoving = false;
    }

    private void OnEnable()
    {
        dropPoint = 5;
        speed = 5.0f;
        moveToTarget = false;
        moveToBox = false;
        isMoving = false;
        targetPosition = new Vector2(GameManager.Instance.ScoreGate.position.x, GameManager.Instance.ScoreGate.position.y);

        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (!moveToTarget)
            return;

        if (rectTransform.position.y <= -100.0f)
        {
            bubbleManager.ScoreCalDrop(dropPoint);
            ObjectPooling.Instance.ReturnToPool(gameObject);
        }
    }

    #region 변수
    public ValueManager.BubbleColor GetBubbleColor()
    {
        return bcolor;
    }
    public GridCell GetGridPos()
    {
        return gridPosition;
    }
    public Color GetBubbleRealColor()
    {
        return sprite.color;
    }

    #endregion

    #region 로직
    public void Init(BubbleManager bubbleManager)
    {
        this.bubbleManager = bubbleManager;
    }

    public void FixPos()
    {
        isMoving = false;
        rb.bodyType = RigidbodyType2D.Static;
    }
    public void SetBubbleColor(ValueManager.BubbleColor color)
    {
        bcolor = color;
        sprite.color = GetRealColor(color);
    }

    public void SetGridPos(GridCell grid)
    {
        gridPosition = grid;
    }

    private Color GetRealColor(ValueManager.BubbleColor color)
    {
        Color colorResult = Color.white;
        switch (color)
        {
            case ValueManager.BubbleColor.Blue:
                colorResult = Color.blue;
                break;
            case ValueManager.BubbleColor.Red:
                colorResult = Color.red;
                break;
            case ValueManager.BubbleColor.Yellow:
                colorResult = Color.yellow;
                break;
            case ValueManager.BubbleColor.Green:
                colorResult = Color.green;
                break;
        }

        return colorResult;
    }

    public void RemoveBubble()
    {
        ObjectPooling.Instance.ReturnToPool(gameObject);
    }

    public void ChangeLayer(string newLayer)
    {
        gameObject.layer = LayerMask.NameToLayer(newLayer);
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        collider.enabled = true;
    }

    #endregion

    #region 발사
    public void SetShootBubbleToGrid(GridCell gridClue)
    {
        bubbleManager.SetShootBubbleToGrid(this, gridClue);
    }

    public void SetShootBubbleToGrid()
    {
        bubbleManager.SetShootBubbleToGrid(this);
    }

    public void SetBubblePhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 50;
    }

    public void BubbleShooted(Transform shootRoot, Vector3 force)
    {
        ReleaseFromArrow(shootRoot);
        rb.AddForce(new Vector2(force.x, force.y), ForceMode2D.Force);
        isMoving = true;
    }

    private void ReleaseFromArrow(Transform shootRoot)
    {
        transform.parent = shootRoot;
    }
    #endregion

    #region 효과
    private void NoneLayerMove()
    {
        moveToTarget = true;
        Vector2 target = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        rb.gravityScale = 1f;
        rb.velocity = target;
        rb.angularVelocity = 0f;
    }

    public void MoveToBox(Transform box)
    {
        Vector2 target = new Vector2(box.position.x - transform.position.x, box.position.y - transform.position.y);
        rb.gravityScale = 1f;
        rb.velocity = target.normalized * speed;
        rb.angularVelocity = 0f;

        ScaleDownCoroutine();
    }

    public void EffectFallingBubble()
    {
        ChangeLayer(ValueManager.LAYER_NONE);
        SetBubblePhysics();
        timer.StartTimerUpdatePercentage(0.5f, () =>
        {
            NoneLayerMove();
        }, null);
    }

    public void EffectExplodeBubble()
    {
        ChangeLayer(ValueManager.LAYER_NONE);

        SetBubblePhysics();
        BubbleShooted(transform.parent, new Vector3(Random.Range(-1000, 1000), Random.Range(-1000, 1000), 0));
        StartCoroutine(ScaleDownCoroutine());
        timer.StartTimerUpdatePercentage(0.5f, () =>
        {
            ObjectPooling.Instance.ReturnToPool(gameObject);
        }, null);
    }

    IEnumerator ScaleDownCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;
        float timeElapsed = 0f;

        while (timeElapsed < 0.5f)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timeElapsed / 0.5f);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
    #endregion

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (isMoving && gameObject.tag.Equals(ValueManager.LAYER_SHOOTBUBBLE))
        {
            //Debug.Log("Hit " + other.collider.name); 
            string nameHit = other.gameObject.tag;

            switch (bcolor.ToString())
            {
                case "Red":
                case "Blue":
                case "Yellow":
                    if (nameHit.Equals(ValueManager.LAYER_BUBBLE) || nameHit.Equals(ValueManager.LAYER_WALL))
                    {
                        gameObject.tag = ValueManager.LAYER_BUBBLE;
                        ChangeLayer(ValueManager.LAYER_BUBBLE);
                        FixPos();

                        if (nameHit.Equals(ValueManager.LAYER_BUBBLE))
                            SetShootBubbleToGrid(other.gameObject.GetComponent<Bubble>().GetGridPos());
                        else
                            SetShootBubbleToGrid();

                        bubbleManager.ExplodeSameColorBubble(this);
                    }
                    break;
                case "Green":
                    if (nameHit.Equals(ValueManager.LAYER_BUBBLE) || nameHit.Equals(ValueManager.LAYER_WALL))
                    {
                        gameObject.tag = ValueManager.LAYER_BUBBLE;
                        ChangeLayer(ValueManager.LAYER_BUBBLE);
                        FixPos();

                        if (nameHit.Equals(ValueManager.LAYER_BUBBLE))
                            SetShootBubbleToGrid(other.gameObject.GetComponent<Bubble>().GetGridPos());
                        else
                            SetShootBubbleToGrid();

                        bubbleManager.ExplodeGreenBubble(this);
                    }
                    break;
            }
        }
    }
}

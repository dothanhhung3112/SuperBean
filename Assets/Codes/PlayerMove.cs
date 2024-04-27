using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    private playerMove playerMove;
    Rigidbody2D rigid;
    CapsuleCollider2D collid;
    Transform trans;
    public float move;
    private float jump;
    public float maxSpeed;
    public float jumpPower;
    SpriteRenderer sprite;
    Animator anim;
    AudioSource audioSource;

    private void Awake()
    {
        trans = GetComponent<Transform>();
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        collid = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        playerMove = new playerMove();
    }

    public void Update()
    {
        //Stop Speed
        //if (Input.GetButton("Horizontal"))
        //{
        //    rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        //}

        //Animation walk
        if (Mathf.Abs(move) < 0.3)
        {
            anim.SetBool("isWalking", false);
        }
        else
        {
            anim.SetBool("isWalking", true);
        }

        //Direction Sprite
        if (move != 0)
        {
            sprite.flipX = move == -1;
        }
    }

    private void FixedUpdate()
    {
        //Move Speed
        rigid.velocity = new Vector2(move * 5, rigid.velocity.y);


        //Max Speed
        if (rigid.velocity.x > maxSpeed)
        {
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        }
        if (rigid.velocity.x < maxSpeed * (-1))
        {
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
        }

        //Landing Platform
        Landing();
    }

    public void Landing()
    {
        //Landing Platform
        if (rigid.velocity.y <= 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                    anim.SetBool("isJumping", false);
            }
        }
    }

    public void ResetAnim()
    {
        anim.SetBool("isJumping", false);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && !anim.GetBool("isJumping"))
        {
            rigid.velocity = new Vector2(rigid.velocity.x, jumpPower);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>().x;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            //Attack
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
                PlaySound("ATTACK");
            }
            else
                OnDamaged(collision.transform.position);
            PlaySound("DAMAGED");
        }

        if (collision.gameObject.tag == "Spike")
        {
            //Attack
            OnDamaged(collision.transform.position);
            PlaySound("DAMAGED");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            //Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");
            if (isBronze)
            {
                gameManager.stagePoint += 50;
            }
            else if (isSilver)
            {
                gameManager.stagePoint += 100;
            }
            else if (isGold)
            {
                gameManager.stagePoint += 300;
            }
            //Deactive Item
            collision.gameObject.SetActive(false);

            //Sound
            PlaySound("ITEM");
        }
        else if (collision.gameObject.tag == "Finish")
        {
            //Next Stage
            gameManager.NextStage();
            PlaySound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        //Point
        gameManager.stagePoint += 100;
        //Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        //Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    void OnDamaged(Vector2 targetPos)
    {
        //Health Down
        gameManager.HealthDown();

        //Change layer(
        gameObject.layer = 9;

        //View alpha
        sprite.color = new Color(1, 1, 1, 0.4f);

        //reaction Force
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 10, ForceMode2D.Impulse);

        anim.SetTrigger("doDamaged");
        Invoke("OffDamaged", 3);
    }

    void OffDamaged()
    {
        gameObject.layer = 8;
        sprite.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        //Sprite alpha
        sprite.color = new Color(1, 1, 1, 0.4f);

        //Sprite Flip Y
        sprite.flipY = true;

        //Collider Disable
        collid.enabled = false;

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        //Sound
        PlaySound("DIE");

    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
        }
        audioSource.Play();
    }
}

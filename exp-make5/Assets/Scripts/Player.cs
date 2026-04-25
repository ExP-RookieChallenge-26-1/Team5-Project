using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // 캐릭터가 바라보는 방향 (움직일 방향)
    public void UpdateDirection(Vector2 direction)
    {
        // 방향 입력이 있을 때 방향 설정
        if (direction != Vector2.zero)
        {
            animator.SetFloat("InputX", direction.x);
            animator.SetFloat("InputY", direction.y);
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null) return;

        float x = 0f;
        float y = 0f;

        // 키보드 화살표로 입력 (임시)
        if (Keyboard.current.upArrowKey.isPressed) y = 1f;
        if (Keyboard.current.downArrowKey.isPressed) y = -1f;
        if (Keyboard.current.rightArrowKey.isPressed) x = 1f;
        if (Keyboard.current.leftArrowKey.isPressed) x = -1f;

        if(x!=0) y=0; //한 방향만 보도록

        Vector2 inputDir = new Vector2(x, y);
        UpdateDirection(inputDir);
    }
}

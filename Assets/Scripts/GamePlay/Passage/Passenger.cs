using UnityEngine;
using DG.Tweening;

public class Passenger : MonoBehaviour
{
    public Color PassengerColor => _passengerColor;
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private BusAndPassageColorManager.BusPassageColors passageColorType;
    [SerializeField] private AudioSource sitSound;

    private Color _passengerColor;
    private bool _isMoving;
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int SittingIdle = Animator.StringToHash("SittingIdle");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Walking = Animator.StringToHash("Walking");
   
    public BusAndPassageColorManager.BusPassageColors PassageColorType => passageColorType;

    public void InitializePassengerColor(BusAndPassageColorManager.BusPassageColors type, Color color)
    {
        passageColorType = type;
        _passengerColor = color;
        meshRenderer.material.color = color;
    }

    public void JumpIntoSeat(BusSeatPoint seatPoint, float jumpPower, float duration)
    {
        if (_isMoving)
            return;

        seatPoint.SetOccupied(this);
        _isMoving = true;
        transform.DOKill();


        transform.SetParent(seatPoint.transform, true);


        Vector3 targetLocalPos = Vector3.zero;
        Quaternion targetLocalRot = Quaternion.identity;


        Sequence seq = DOTween.Sequence();

        seq.OnStart(PlayJumpAnimation);

        seq.OnComplete(PlaySittingAnimation);

        seq.Append(transform.DOLocalJump(targetLocalPos, jumpPower, 1, duration)
            .SetEase(Ease.OutQuad));


        seq.Join(transform.DOLocalRotateQuaternion(targetLocalRot, duration * 0.5f));

        seq.OnComplete(() =>
        {
            transform.localPosition = targetLocalPos;
            transform.localRotation = targetLocalRot;


            sitSound.Play();

            _isMoving = false;
        });
    }

    public void PlayWalkAnimation()
    {
        animator.SetBool(Walking, true);
    }

    public void StopWalkAnimation()
    {
        animator.SetBool(Walking, false);
    }

    public void PlayJumpAnimation()
    {
        animator.SetTrigger(Jump);
    }

    private void StopJumpAnimation()
    {
        animator.ResetTrigger(Jump);
    }

    public void PlaySittingAnimation()
    {
        animator.SetTrigger(SittingIdle);
    }
}
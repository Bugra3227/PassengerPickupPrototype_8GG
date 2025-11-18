using UnityEngine;
using DG.Tweening;

public class Passenger : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private BusAndPassageColorManager.BusPassageColors passageColorType;
    [SerializeField] private AudioSource sitSound;

    private Color _passengerColor;
    private bool _isMoving;
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int SittingIdle = Animator.StringToHash("SittingIdle");

    public BusAndPassageColorManager.BusPassageColors PassageColorType => passageColorType;

    public void InitializePassengerColor(BusAndPassageColorManager.BusPassageColors type, Color color)
    {
        passageColorType = type;
        _passengerColor = color;

        if (meshRenderer != null)
        {
            meshRenderer.material = new Material(meshRenderer.material);
            meshRenderer.material.color = color;
        }
    }

    public void JumpIntoSeat(BusSeatPoint seatPoint, float jumpPower, float duration)
    {
        if (_isMoving)
            return;
        seatPoint.SetOccupied(this);
        _isMoving = true;
        transform.DOKill();


        animator.ResetTrigger(Idle);
        animator.SetTrigger(Jump);


        Vector3 targetPos = seatPoint.transform.position;
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOJump(targetPos, jumpPower, 1, duration)
            .SetEase(Ease.OutQuad));

        seq.Join(transform.DOLookAt(targetPos, duration * 0.5f));

        seq.OnComplete(() =>
        {
            transform.SetParent(seatPoint.transform, true);
            transform.position = seatPoint.transform.position;
            transform.rotation = seatPoint.transform.rotation;

            if (animator != null)
            {
                animator.ResetTrigger(Jump);
                animator.SetTrigger(SittingIdle);
            }

            sitSound.Play();
            _isMoving = false;
        });
    }
}
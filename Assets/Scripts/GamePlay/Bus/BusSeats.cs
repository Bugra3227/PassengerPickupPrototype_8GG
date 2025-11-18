using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween kütüphanesini dahil et

public class BusSeats : MonoBehaviour
{
    [Header("Seat Management")] private readonly List<BusSeatPoint> seats = new List<BusSeatPoint>();
    [SerializeField] private BusConfig busConfig;
    [SerializeField] private Transform effectTransform;
    [SerializeField] private AudioSource effectSound;

    [Header("Full Bus Effects")] [SerializeField]
    private float jumpHeight = 1f; // Zıplama yüksekliği

    [SerializeField] private float jumpDuration = 0.5f; // Zıplama süresi
    [SerializeField] private float scaleDownDuration = 0.8f; // Küçülme süresi
    [SerializeField] private ParticleSystem finishEffectPrefab; // Bitiş Parçacık Efekti (Inspector'dan atanacak)

    public BusAndPassageColorManager.BusPassageColors BusPassageColorEnum => busConfig.BusPassageColorEnums;

    private bool isBusFull = false;

   

    public void AddSeatPointToList()
    {
        seats.Clear();
        seats.AddRange(GetComponentsInChildren<BusSeatPoint>(true));
    }

    public BusSeatPoint FindEmptySeat()
    {
        BusSeatPoint point = null;
        for (int i = 0; i < seats.Count; i++)
        {
            if (!seats[i].IsOccupied)
            {
                point = seats[i];
                break;
            }
        }
        
        if (point == null)
        {
            CheckFullBus();
        }

        return point;
    }

    public void CheckFullBus()
    {
        
        if (isBusFull) return;

        bool allSeatsOccupied = true;
        foreach (var seat in seats)
        {
            if (!seat.IsOccupied)
            {
                allSeatsOccupied = false;
                break;
            }
        }

        if (allSeatsOccupied)
        {
            isBusFull = true;
            StartDisappearEffect();
        }
    }

    private void StartDisappearEffect()
    {
       
    
        float originalY = transform.position.y;
        float moveUpDuration = jumpDuration * 0.5f; 

        Sequence seq = DOTween.Sequence();
    
        seq.Append(transform.DOMoveY(originalY + jumpHeight, jumpDuration)
            .SetEase(Ease.Linear));

        seq.Append(transform.DOScale(Vector3.one*0.1f, scaleDownDuration)
            .SetEase(Ease.Linear)); 
        
        seq.OnComplete(() =>
        {
            if (finishEffectPrefab != null)
            {
                ParticleSystem effect = Instantiate(finishEffectPrefab, effectTransform.position, Quaternion.identity);
                effect.Play();
            }
            effectSound.Play();
            Destroy(gameObject,0.25f);
            BusManager.Instance.IncreaseTotalFullBus();
            
        });
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public Animator[] healthItems;
    public Animator geo;
    public float showHealthItemIntervalTime = 0.2f;

    private BaseEntity BaseEntity;

    private void Start()
    {
        BaseEntity = FindFirstObjectByType<PlayerAttack>();
    }

    public void Hurt()
    {
        if (BaseEntity.GetIsDead())
            return;
        BaseEntity.LoseHealth(1);
        int health = BaseEntity.GetCurrentHealth();
        healthItems[health].SetTrigger("Hurt");
    }

    public IEnumerator ShowHealthItems()
    {
        for (int i = 0; i < healthItems.Length; i++)
        {
            healthItems[i].SetTrigger("Respawn");
            yield return new WaitForSeconds(showHealthItemIntervalTime);
        }
        yield return new WaitForSeconds(showHealthItemIntervalTime);
        geo.Play("Enter");
    }

    public void HideHealthItems()
    {
        geo.Play("Quit");
        for (int i = 0; i < healthItems.Length; i++)
        {
            healthItems[i].SetTrigger("Hide");
        }
    }
    public void OnEntityHurt(BaseEntity entity)
    {
        if (entity != playerEntity) return;

        int health = entity.GetCurrentHealth();
        if (entity.GetIsDead())
        {
            foreach (var item in healthItems)
                item.SetTrigger("Hide");
        }
        else
        {
            healthItems[health].SetTrigger("Hurt");
        }
    }
}

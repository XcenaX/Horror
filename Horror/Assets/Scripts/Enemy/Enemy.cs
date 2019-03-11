using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // <-- add this line
using UnityEngine.SceneManagement;
public class Enemy : MonoBehaviour
{
    [Header("Nav mesh settings")]
    [SerializeField]
    private Transform player;

    [SerializeField]
    private NavMeshAgent navMesh;

    [SerializeField]
    private Animator anim;

    // состояние врага
    private string state = "Idle";
    private bool isAlive = true;

    [Header("Enemy settings")]
    [SerializeField]
    private float searchRadius;

    [SerializeField]
    private float waitTime;
    private float wait;

    [SerializeField]
    private Transform triggerZone;

    [SerializeField]
    private AudioSource audio;

    private bool highAlert = false;
   private float alertLevel = 0; // уровень тревоги
   [SerializeField]
   private  Transform deathCamera;

   [SerializeField]
   private Transform deathCameraPosition;

   [SerializeField]
   private Transform enemyPrefab;

    private void Start(){
        navMesh.speed = 1;
        anim.speed = 1;
    }

    private void Update(){
        if(isAlive == false)
            return; 
            if(navMesh != null)       
        anim.SetFloat("speed", navMesh.velocity.magnitude);

        var countOfItems = player.GetComponent<InteractManager>().GetCountOfItems();
        var countOfItemsToKillEnemy = player.GetComponent<InteractManager>().GetCountOfItemsToKillEnemy();
        if(countOfItems >= countOfItemsToKillEnemy){
            KillEnemy();            
        }

        switch(state){
            case "Idle":                   
                GoToRandomPoint();
            break;
            case "Walk":
                CheckDistance();
            break;
            case "Search":
                Search();
            break;
            case "Chase":
                ChaseForPlayer();
            break;
            case "Hunt":
                CheckDistance();
            break;
            case "Kill":
                deathCamera.position = Vector3.Slerp(deathCamera.position, deathCameraPosition.position, 10f*Time.deltaTime);
                deathCamera.rotation = Quaternion.Slerp(deathCamera.rotation, deathCameraPosition.rotation, 10f*Time.deltaTime);
                anim.speed = 0.4f;
            break;
            default:
            break;
        }
    }

    private void ChaseForPlayer(){
        navMesh.SetDestination(player.position);
        float distance = Vector3.Distance(transform.position, player.position);
        if(distance > 10f){
            state = "Hunt";
            highAlert = true;
            alertLevel = 20;
        }else if(navMesh.remainingDistance <= navMesh.stoppingDistance && navMesh.pathPending == false){            
            var playerController = player.GetComponent<PlayerController>();
            if(playerController.isAlive == true){
                state = "Kill";
                KillPlayer();
            }
        }
    }

    private void KillPlayer(){
        anim.SetTrigger("Kill");
        var playerController = player.GetComponent<PlayerController>();
        playerController.KillPlayer();
        deathCamera.gameObject.SetActive(true);
        deathCamera.transform.position = Camera.main.transform.position;
        deathCamera.transform.rotation = Camera.main.transform.rotation;

        Camera.main.gameObject.SetActive(false);
        audio.pitch = 0.8f;
        audio.Play();
        Invoke("RestartLevel", 2f);
    }

    private void RestartLevel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CheckSight(){
        if(isAlive == false)
            return;
        
        RaycastHit hit;
        if(Physics.Linecast(triggerZone.position, player.position, out hit)){            

        if(hit.collider.tag == "Player"){
            if(state != "Kill"){
                state = "Chase";
                navMesh.speed = 2;
                anim.speed = 2;
                audio.Play();
            }
        }

        }
    }

    private void GoToRandomPoint(){
        // генерируем случайную позицию внутри сферы
        Vector3 randomPos = Random.insideUnitSphere * searchRadius;
        NavMeshHit navHit;
        NavMesh.SamplePosition(
            transform.position + randomPos, 
            out navHit, 
            searchRadius, 
            NavMesh.AllAreas
        );

        if(highAlert){
            NavMesh.SamplePosition(
            player.position, 
            out navHit, 
            searchRadius, 
            NavMesh.AllAreas            
        );
        alertLevel -=5;
        if(alertLevel <= 0){
            highAlert = false;
            navMesh.speed = 1;
            anim.speed = 1;
        }
        }
        if(navMesh != null)navMesh.SetDestination(navHit.position);
        state = "Walk";
    }

    private void CheckDistance(){
        if(navMesh == null)return;
        var remainingDistance = navMesh.remainingDistance;
        var stoppingDistance = navMesh.stoppingDistance;
        // когда достигли цели
        if(remainingDistance <= stoppingDistance && navMesh.pathPending == false){
            state = "Search";
            wait = waitTime;
        }
    }

    private void Search(){
        if(wait <= 0){
            state = "Idle";
            return;
        }

        wait -= Time.deltaTime;
        transform.Rotate(0, 120f * Time.deltaTime ,0);
    }

    public void SetPlayer(Transform player){
        this.player = player;
    }
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }

    private void OnTriggerStay(Collider other)
    {        
        var flash = player.GetComponent<FlashLight>();        
        if(other.tag == "FlashLight" && flash.IsEnabled() == true){           
            KillEnemy();
        }
    }

    public void KillEnemy(){
        Vector3 rand;
        while(true){                
            Vector3 randomPos = Random.insideUnitCircle * 30f;
            NavMeshHit navHit;
            NavMesh.SamplePosition(
            player.position + randomPos, 
            out navHit, 
            searchRadius, 
            NavMesh.AllAreas
            );
            rand = player.position + randomPos;
            if(Vector3.Distance(rand , player.position) > 20f)break;
            }
            rand.y = 1;

            if(player.GetComponent<InteractManager>().GetCountOfItems() < player.GetComponent<InteractManager>().GetCountOfItemsToKillEnemy()){
                var enemy = Instantiate(enemyPrefab,rand, Quaternion.identity);   
                enemy.GetComponent<Enemy>().SetPlayer(player);         
            }
             Destroy(transform.gameObject);
    }
}

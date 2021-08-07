using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyGameStudio.Jeremy
{
    public class Selected_effect_touch : MonoBehaviour
    {
        public bool is_selected = false;

        private bool is_touched = false;

        private Material material;

        // Start is called before the first frame update
        void Start()
        {
            if (this.GetComponent<MeshRenderer>() == null)
            {
                this.material = this.GetComponent<SkinnedMeshRenderer>().material;
            }
            else
            {
                this.material = this.GetComponent<MeshRenderer>().material;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount >= 1)
            {
                //获取触摸位置
                Touch touch = Input.touches[0];
                Vector3 pos = touch.position;

                //发射射线
                #region 
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(pos);
                if (Physics.Raycast(ray, out hit) && (hit.transform.name == this.gameObject.name))
                {
                    this.is_touched = true;
                }
                else
                {
                    this.is_touched = false;
                }
                #endregion

                //抬起手指退出拖拽
                #region 
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    if (this.is_touched)
                    {
                        if (this.is_selected == true)
                        {
                            this.change_to_not_selected();
                        }
                        else
                        {
                            this.change_to_selected();
                        }
                    }
                }
                #endregion
            }








        }

        private void change_to_selected()
        {
            if (this.is_selected != true)
            {
                this.is_selected = true;
                GameObject.FindGameObjectWithTag("audio_selected").GetComponent<AudioSource>().Play();
            }
        
            if (this.material.GetInt("is_selected") != 1)
                this.material.SetInt("is_selected", 1);
        }

        private void change_to_not_selected()
        {
            if (this.is_selected != false)
            {
                this.is_selected = false;
                GameObject.FindGameObjectWithTag("audio_selected").GetComponent<AudioSource>().Play();
            }

            if (this.material.GetInt("is_selected") != 0)
                this.material.SetInt("is_selected", 0);
        }
    }
}
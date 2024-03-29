﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Skylight;
using System;

public class TalkEvent : EventArgs
{
    public string m_who;
    public string m_content;
    public float m_delayTime;
    public bool m_isKeyTalk;
}
public class ActionEvent : MonoBehaviour
{

    ActionEvent()
    {
    }

    public void  LoadCSV(string CSVName)
    {
        StartCoroutine(LoadCSV(CSVName, OnFileLoaded));
    }

    public void OnFileLoaded()
    {
        m_isLoaded = true;
    }

    public bool IsFinished()
    {
        return m_isLoaded && m_currentIndex == m_ArrayData.Length;
    }

    public void InternalUpdate()
    {
        if (!m_isLoaded)
        {
            return;
        }
        if (IsFinished())
        {
            return;
        }
        if (IsRunning())
        {
            return;
        }

        PlayStoryByLine(m_currentIndex);
        m_currentIndex++;
    }


    private EActor GetActorByName(string name)
    {
        switch (name)
        {
            case "teacher":
                return EActor.Teacher;
            case "girl":
                return EActor.Girl;
            default:
                Assert.IsTrue(false);
                break;
        }
        return EActor.Null;
    }

    private ELocation GetLocationByName(string name)
    {
        switch (name)
        {
            case "desk":
                return ELocation.Desk;
            case "chair":
                return ELocation.Chair;
            case "window":
                return ELocation.Window;
            default:
                Assert.IsTrue(false);
                break;
        }
        return ELocation.Null;
    }

    private EInteraction GetInteractionByName(string name)
    {
        switch (name)
        {
            case "paper":
                return EInteraction.Paper;
            case "window":
                return EInteraction.Window;
            case "phone":
                return EInteraction.Phone;
            default:
                Assert.IsTrue(false);
                break;
        }
        return EInteraction.Null;
    }
    private IEnumerator LoadCSV(string fileName, UnityAction CompleteAction)
    {
        string sPath = Application.streamingAssetsPath + "/" + fileName;
        Debug.Log("sPath:" + sPath);
        WWW www = new WWW(sPath);
        while (!www.isDone)
        {
            yield return null;
        }
        Debug.Log("Content Review: \n" + www.text);
        File.WriteAllText(Application.persistentDataPath + "/" + fileName, www.text, Encoding.GetEncoding("utf-8"));
        LoadFile(Application.persistentDataPath, fileName);
        CompleteAction();
    }

    private void LoadFile(string path, string fileName)
    {
        m_ArrayData = new string[0][];
        string fillPath = path + "/" + fileName;

        string[] lineArray;
        try
        {
            lineArray = File.ReadAllLines(fillPath, Encoding.GetEncoding("utf-8"));
            Debug.Log("file finded!");
        }
        catch
        {
            Debug.Log("file do not find!");
            return;
        }

        m_ArrayData = new string[lineArray.Length][];
        for (int i = 0; i < lineArray.Length; i++)
        {
            m_ArrayData[i] = lineArray[i].Split(',');
        }
    }

    private string GetVaule(int row, int col)
    {
        return m_ArrayData[row][col];
    }

    private bool IsRunning()
    {
        if (m_countDown > 0)
        {
            m_countDown -= Time.deltaTime;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void InterRuningState(float countDown)
    {
        Assert.IsTrue(countDown > 0);
        Assert.IsTrue(m_countDown < 0);
        m_countDown = countDown;
    }

    private void PlayStoryByLine(int m_currentIndex)
    {
        string[] command = m_ArrayData[m_currentIndex];

        switch (command[0])
        {
            case "move":
            case "Move":
                {
                    Assert.IsTrue(command.Length == 3);
                    string who = command[1];
                    string where = command[2];

                    EActor actor = GetActorByName(who);
                    ELocation location = GetLocationByName(where);

                    GameObject actorGO = StoryManager.Instance().GetActorGameobjectByEActor(actor);
                    GameObject locationGO = StoryManager.Instance().GetLocationGameobjectByELocation(location);

                    NPCController controller = actorGO.GetComponent<NPCController>();
                    Assert.IsNotNull(controller);
                    if (controller)
                    {
                        float durationTime = controller.MoveToActionPoint(locationGO);
                        InterRuningState(durationTime);
                    }
                }

                break;
            case "interact":
            case "Interact":
                {
                    Assert.IsTrue(command.Length == 3);

                    string who = command[1];
                    string what = command[2];

                    EActor actor = GetActorByName(who);
                    EInteraction location = GetInteractionByName(what);
                    GameObject actorGO = StoryManager.Instance().GetActorGameobjectByEActor(actor);
                    GameObject locationGO = StoryManager.Instance().GetINteractionGameobjectByEInteraction(location);

                    // TODO: Play actor animation.
                    locationGO.GetComponent<Interaction>().PlayAnimation();

                }

                break;
            case "talk":
            case "Talk":
                {
                    TalkExecute(command);
                }

                break;
            case "talkKey":
            case "talkkey":
            case "TalkKey":
            case "Talkkey":
                {
                    TalkExecute(command, true);
                }

                break;
            default:
                break;
        }
    }

    private void TalkExecute(string[] command, bool isKeyTalk = false)
    {
        Assert.IsTrue(command.Length > 3);

        GameRuntimeSetting.ELanguage language = GameRuntimeSetting.Instance().GetCurrentLanguage();

        string who = "";
        string saySomething = "";
        for (int i = 1; i < command.Length; i++)
        {
            switch (language)
            {
                case GameRuntimeSetting.ELanguage.Chinese:
                    {
                        if (command[i] == "cn")
                        {
                            who = command[i + 1];
                            saySomething = command[i + 2];
                        }
                    }
                    break;
                case GameRuntimeSetting.ELanguage.Japanese:
                    {
                        if (command[i] == "jp")
                        {
                            who = command[i + 1];
                            saySomething = command[i + 2];
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        Assert.IsTrue(who.Length > 0);
        Assert.IsTrue(saySomething.Length > 0);

        float delayTime = saySomething.Length * 0.5f + 2f;
        StarPlatinum.EventManager.EventManager.Instance.SendEvent(new TalkEvent { m_who = who, m_content = saySomething, m_delayTime = delayTime, m_isKeyTalk = isKeyTalk });
        InterRuningState(delayTime);
    }

    [SerializeField]
    private string m_fileName;

    private bool m_isLoaded = false;
    private int m_currentIndex = 0;
    private string[][] m_ArrayData;

    [SerializeField]
    private float m_countDown = -1;
}

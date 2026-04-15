using TMPro;
using UnityEngine;

public class QuestPanelUI : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private QuestData questData;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text guideText;

    [Header("Input")]
    [SerializeField] private KeyCode nextKey = KeyCode.Space;

    [Header("Player")]
    [SerializeField] private PlayerActionLock playerActionLock;

    private QuestData currentDisplayQuestData;
    private QuestDialogueData currentDialogue;
    
    private int currentIndex;
    private bool isPlaying;

    private void OnEnable()
    {
        StartDialogue();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (Input.GetKeyDown(nextKey))
            NextLine();
    }

    private void StartDialogue()
    {
        if (playerActionLock != null)
            playerActionLock.LockRecoverControls();

        currentDisplayQuestData = GetDisplayQuestData();

        if (currentDisplayQuestData == null)
        {
            ClosePanel();
            return;
        }

        currentDialogue = GetCurrentDialogue(currentDisplayQuestData);

        if (currentDialogue == null || currentDialogue.lines == null || currentDialogue.lines.Count == 0)
        {
            ClosePanel();
            return;
        }

        currentIndex = 0;
        isPlaying = true;

        if (guideText != null)
            guideText.text = "SPACE";

        RefreshText();
    }

    private QuestDialogueData GetCurrentDialogue(QuestData displayQuestData)
    {
        if (displayQuestData == null)
            return null;

        QuestStateData state = QuestManager.Instance != null
            ? QuestManager.Instance.GetState(displayQuestData.questId)
            : null;

        // 아직 안 받음
        if (state == null || !state.isAccepted)
            return displayQuestData.startDialogue;

        // 진행 중
        if (!state.isCompleted)
            return displayQuestData.progressDialogue != null
                ? displayQuestData.progressDialogue
                : displayQuestData.startDialogue;

        // 완료했지만 보상 안 받음
        if (!state.isRewardClaimed)
            return displayQuestData.completeDialogue != null
                ? displayQuestData.completeDialogue
                : displayQuestData.progressDialogue;

        // 여기까지 왔으면 이 퀘스트는 끝난 상태
        return displayQuestData.noQuestDialogue;
    }

    private void NextLine()
    {
        currentIndex++;

        if (currentDialogue == null || currentIndex >= currentDialogue.lines.Count)
        {
            FinishDialogue();
            return;
        }

        RefreshText();
    }

    private void RefreshText()
    {
        if (dialogueText == null || currentDialogue == null)
            return;

        dialogueText.text = currentDialogue.lines[currentIndex];
    }

    private void FinishDialogue()
    {
        isPlaying = false;

        if (currentDisplayQuestData != null && QuestManager.Instance != null)
        {
            QuestStateData state = QuestManager.Instance.GetState(currentDisplayQuestData.questId);

            if (state == null || !state.isAccepted)
            {
                QuestManager.Instance.AcceptQuest(currentDisplayQuestData);
            }
            else if (state.isCompleted && !state.isRewardClaimed)
            {
                QuestService.ClaimReward(currentDisplayQuestData);
            }
        }

        ClosePanel();
    }

    private QuestData GetDisplayQuestData()
    {
        if (questData == null)
            return null;

        QuestData currentQuest = questData;

        while (currentQuest != null)
        {
            QuestStateData state = QuestManager.Instance != null
                ? QuestManager.Instance.GetState(currentQuest.questId)
                : null;

            // 아직 안 받은 퀘스트면 이걸 보여줌
            if (state == null || !state.isAccepted)
                return currentQuest;

            // 진행 중이거나 완료 보상 전이면 이것도 현재 퀘스트
            if (!state.isRewardClaimed)
                return currentQuest;

            // 보상까지 끝났으면 다음 퀘스트 검사
            currentQuest = currentQuest.nextQuest;
        }

        return null;
    }

    private void ClosePanel()
    {
        if (playerActionLock != null)
            playerActionLock.UnlockRecoverControls();

        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}
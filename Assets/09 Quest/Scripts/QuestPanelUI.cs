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

    [Header("Fallback Dialogue")]
    [SerializeField] private QuestDialogueData progressDialogue;
    [SerializeField] private QuestDialogueData noQuestDialogue;

    [Header("Input")]
    [SerializeField] private KeyCode nextKey = KeyCode.Space;

    [Header("Player")]
    [SerializeField] private PlayerActionLock playerActionLock;

    private QuestData currentDisplayQuestData;
    private QuestDialogueData currentDialogue;
    
    private int currentIndex;
    private bool isPlaying;
    private bool isNoQuestMode;

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

            // 아직 안 받은 퀘스트
            if (state == null || !state.isAccepted)
                return currentQuest;

            // 진행 중이거나 완료했지만 보상 안 받음
            if (!state.isRewardClaimed)
                return currentQuest;

            // 보상까지 받았으면 다음 퀘스트 검사
            currentQuest = currentQuest.nextQuest;
        }

        // 전부 끝난 상태
        return null;
    }

    private QuestDialogueData GetCurrentDialogue(QuestData displayQuestData)
    {
        isNoQuestMode = false;

        // 더 이상 줄 퀘스트가 없으면 공통 no quest 대사
        if (displayQuestData == null)
        {
            isNoQuestMode = true;
            return noQuestDialogue;
        }

        QuestStateData state = QuestManager.Instance != null
            ? QuestManager.Instance.GetState(displayQuestData.questId)
            : null;

        // 아직 안 받은 상태 → 시작 대사
        if (state == null || !state.isAccepted)
            return displayQuestData.startDialogue;

        // 진행 중 → 공통 진행중 대사
        if (!state.isCompleted)
            return progressDialogue;

        // 완료했지만 보상 안 받음 → 개별 완료 대사
        if (!state.isRewardClaimed)
            return displayQuestData.completeDialogue;

        // 여기까지 오면 사실상 no quest
        isNoQuestMode = true;
        return noQuestDialogue;
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

        if (currentIndex < 0 || currentIndex >= currentDialogue.lines.Count)
            return;

        dialogueText.text = currentDialogue.lines[currentIndex];
    }

    private void FinishDialogue()
    {
        isPlaying = false;

        // "임무 없음" 대사면 아무 처리 없이 닫기
        if (isNoQuestMode)
        {
            ClosePanel();
            return;
        }

        if (currentDisplayQuestData != null && QuestManager.Instance != null)
        {
            QuestStateData state = QuestManager.Instance.GetState(currentDisplayQuestData.questId);

            // 아직 안 받았으면 수락
            if (state == null || !state.isAccepted)
            {
                QuestManager.Instance.AcceptQuest(currentDisplayQuestData);
            }
            // 완료했고 보상 안 받았으면 보상 지급
            else if (state.isCompleted && !state.isRewardClaimed)
            {
                QuestService.ClaimReward(currentDisplayQuestData);
            }
        }

        ClosePanel();
    }

    private void ClosePanel()
    {
        if (playerActionLock != null)
            playerActionLock.UnlockRecoverControls();

        if (panelRoot != null)
            UIManager.Instance.ClosePanel(UIPanelType.Quest);
        else
            gameObject.SetActive(false);
    }
}
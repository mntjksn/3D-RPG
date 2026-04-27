using TMPro;
using UnityEngine;

// 퀘스트 대사 표시 및 수락 / 보상 처리 담당
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
        if (!isPlaying) return;

        if (Input.GetKeyDown(nextKey))
            NextLine();
    }

    // 대사 시작
    private void StartDialogue()
    {
        currentDisplayQuestData = GetDisplayQuestData();
        currentDialogue = GetCurrentDialogue(currentDisplayQuestData);

        if (currentDialogue == null || currentDialogue.lines == null || currentDialogue.lines.Count == 0)
        {
            ClosePanel();
            return;
        }

        currentIndex = 0;
        isPlaying = true;

        guideText?.SetText("SPACE");
        RefreshText();
    }

    // 현재 표시할 퀘스트 결정
    private QuestData GetDisplayQuestData()
    {
        if (questData == null)
            return null;

        QuestData currentQuest = questData;

        while (currentQuest != null)
        {
            QuestStateData state = QuestManager.Instance?.GetState(currentQuest.questId);

            // 아직 안 받은 퀘스트
            if (state == null || !state.isAccepted)
                return currentQuest;

            // 진행 중이거나 완료 후 보상 미수령
            if (!state.isRewardClaimed)
                return currentQuest;

            // 다음 퀘스트 확인
            currentQuest = currentQuest.nextQuest;
        }

        return null;
    }

    // 현재 상황에 맞는 대사 결정
    private QuestDialogueData GetCurrentDialogue(QuestData displayQuestData)
    {
        isNoQuestMode = false;

        if (displayQuestData == null)
        {
            isNoQuestMode = true;
            return noQuestDialogue;
        }

        QuestStateData state = QuestManager.Instance?.GetState(displayQuestData.questId);

        // 아직 안 받은 상태
        if (state == null || !state.isAccepted)
            return displayQuestData.startDialogue;

        // 진행 중
        if (!state.isCompleted)
            return progressDialogue;

        // 완료 후 보상 미수령
        if (!state.isRewardClaimed)
            return displayQuestData.completeDialogue;

        isNoQuestMode = true;
        return noQuestDialogue;
    }

    // 다음 대사로 이동
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

    // 현재 대사 표시
    private void RefreshText()
    {
        if (dialogueText == null || currentDialogue == null)
            return;

        if (currentIndex < 0 || currentIndex >= currentDialogue.lines.Count)
            return;

        dialogueText.SetText(currentDialogue.lines[currentIndex]);
    }

    // 대사 종료 처리
    private void FinishDialogue()
    {
        isPlaying = false;

        // 임무 없음 대사는 바로 닫기
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

    // 패널 닫기
    private void ClosePanel()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Quest);
    }
}
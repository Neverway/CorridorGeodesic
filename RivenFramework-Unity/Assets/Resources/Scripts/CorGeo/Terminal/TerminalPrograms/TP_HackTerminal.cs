using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TP_HackTerminal : TerminalProgramBase
{
    public HackPuzzleDefinition puzzleDefinition;
    
    public Transform gridContainer;
    public WB_HackCell cellPrefab;

    public Transform intersectionContainer;
    public WB_HackIntersection intersectionPrefab;
    
    public Button collapseButton;
    public Button expandButton;
    public Button stepButton;
    public Button resetButton;

    public TMP_Text accumulatorText;
    public TMP_Text accumulatorSubText;
    public TMP_Text attemptsText;
    public TMP_Text messageText;

    public float stepDelay = 0.5f;

    public float cellSize = 64f;
    
    private int rows, cols;
    private HackCell[][] grid;

    private readonly Dictionary<(int, int), WB_HackCell> cellViews = new Dictionary<(int, int), WB_HackCell>();
    private readonly Dictionary<(int, int), WB_HackIntersection> intersectionViews = new Dictionary<(int, int), WB_HackIntersection>();

    private readonly List<(int rowLine, int colLine)> selectedPoints = new List<(int, int)>();
    private ((int rowLine, int colLine) a, (int rowLine, int colLine) b)? activeCollapse;
    private bool[,] hidden;
    
    private int accumulator;
    private int attemptsRemaining;
    private bool running, won, locked;

    /// <summary>
    /// Initialize the minigame values when the program is launched
    /// </summary>
    protected override void OnLaunch()
    {
        rows = puzzleDefinition != null ? puzzleDefinition.rows : 4;
        cols = puzzleDefinition != null ? puzzleDefinition.cols : 4;
        grid = puzzleDefinition != null ? puzzleDefinition.BuildGrid() : BuildFallbackGrid();

        hidden = new bool[rows, cols];
        activeCollapse = null;
        selectedPoints.Clear();

        accumulator = 0;
        attemptsRemaining = puzzleDefinition != null ? puzzleDefinition.startingAttempts : 4;
        running = false; won = false; locked = false;

        collapseButton.onClick.AddListener(OnCollapse);
        expandButton.onClick.AddListener(OnExpand);
        stepButton.onClick.AddListener(OnStep);
        resetButton.onClick.AddListener(OnReset);

        BuildGridViews();
        BuildIntersectionViews();
        RenderAll();
    }

    /// <summary>
    /// Disconnect the subscribed listeners when the hack program is closed
    /// </summary>
    protected override void OnTerminate()
    {
        collapseButton.onClick.RemoveListener(OnCollapse);
        expandButton.onClick.RemoveListener(OnExpand);
        stepButton.onClick.RemoveListener(OnStep);
        resetButton.onClick.RemoveListener(OnReset);
    }

    /// <summary>
    /// If the hack puzzle is missing, malformed, or somehow just not working, use this basic test memory grid
    /// </summary>
    private HackCell[][] BuildFallbackGrid()
    {
        var grid = new HackCell[4][];
        for (int r = 0; r < 4; r++)
        {
            grid[r] = new HackCell[4];
            for (int c = 0; c < 4; c++)
            {
                grid[r][c] = HackCell.Nop();
            }
        }
        grid[1][0] = HackCell.Adc();
        grid[1][1] = HackCell.Adc();
        grid[1][2] = HackCell.CmpSecurity(8);
        grid[1][3] = HackCell.Jmp(3, 0);
        grid[2][0] = HackCell.Adc();
        grid[2][1] = HackCell.Adc();
        grid[2][2] = HackCell.Adc();
        grid[2][3] = HackCell.Adc();
        grid[3][0] = HackCell.Adc();
        grid[3][1] = HackCell.Adc();
        grid[3][2] = HackCell.CmpGoal(16);
        return grid;
    }
    
    private Vector2 CellOrigin(int r, int c) => new Vector2(c * cellSize, -r * cellSize);

    private Vector2 CellCenter(int r, int c) => CellOrigin(r, c) + new Vector2(cellSize / 2f, -cellSize / 2f);
    
    private Vector2 IntersectionPosition(int rowLine, int colLine) => new Vector2(colLine * cellSize, -rowLine * cellSize);
    
    private void BuildGridViews()
    {
        foreach (Transform child in gridContainer) Destroy(child.gameObject);
        cellViews.Clear();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var view = Instantiate(cellPrefab, gridContainer);
                view.rect.anchoredPosition = CellOrigin(r, c);
                view.SetHidden(false, animate: false);
                cellViews[(r, c)] = view;
            }
        }
    }

    private void BuildIntersectionViews()
    {
        foreach (Transform child in intersectionContainer) Destroy(child.gameObject);
        intersectionViews.Clear();

        for (int rowLine = 0; rowLine <= rows; rowLine++)
        {
            for (int colLine = 0; colLine <= cols; colLine++)
            {
                var marker = Instantiate(intersectionPrefab, intersectionContainer);
                marker.rect.anchoredPosition = IntersectionPosition(rowLine, colLine);
                marker.Bind(rowLine, colLine, OnIntersectionClicked);
                intersectionViews[(rowLine, colLine)] = marker;
            }
        }
    }

    private void OnIntersectionClicked(int rowLine, int colLine)
    {
        if (running || locked || won) return;
        if (activeCollapse.HasValue) return;

        var point = (rowLine, colLine);
        int idx = selectedPoints.IndexOf(point);
        if (idx != -1) selectedPoints.RemoveAt(idx);
        else if (selectedPoints.Count < 2) selectedPoints.Add(point);
        else { selectedPoints.Clear(); selectedPoints.Add(point); }

        RefreshIntersectionVisuals();
        RefreshSelectionPreview();
        UpdateControlInteractable();
    }

    private void RefreshIntersectionVisuals()
    {
        foreach (var kvp in intersectionViews)
            kvp.Value.SetSelected(selectedPoints.Contains(kvp.Key));
    }

    private void RefreshSelectionPreview()
    {
        bool showPreview = selectedPoints.Count == 2 && !activeCollapse.HasValue;
        bool[,] previewed = showPreview ? ComputeSelection(selectedPoints[0], selectedPoints[1]) : new bool[rows, cols];

        foreach (var kvp in cellViews)
        {
            var (r, c) = kvp.Key;
            kvp.Value.SetPreviewSelected(showPreview && previewed[r, c]);
        }
    }

    private bool[,] ComputeSelection((int rowLine, int colLine) a, (int rowLine, int colLine) b)
    {
        var result = new bool[rows, cols];

        Vector2 posA = IntersectionPosition(a.rowLine, a.colLine);
        Vector2 posB = IntersectionPosition(b.rowLine, b.colLine);
        Vector2 delta = posB - posA;
        float len = delta.magnitude;
        if (len < 0.001f) return result;

        Vector2 n = delta / len;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float t = Vector2.Dot(CellCenter(r, c) - posA, n);
                result[r, c] = t >= 0f && t <= len;
            }
        }
        return result;
    }


    private void OnCollapse()
    {
        if (running || locked || won) return;
        if (activeCollapse.HasValue) return;
        if (selectedPoints.Count != 2) return;

        activeCollapse = (selectedPoints[0], selectedPoints[1]);
        hidden = ComputeSelection(activeCollapse.Value.a, activeCollapse.Value.b);

        ApplyCollapseVisuals(animate: true);
        RefreshSelectionPreview();
        UpdateControlInteractable();
    }

    private void OnExpand()
    {
        if (running || locked || won) return;
        if (!activeCollapse.HasValue) return;

        activeCollapse = null;
        hidden = new bool[rows, cols];
        selectedPoints.Clear();

        ApplyCollapseVisuals(animate: true);
        RefreshIntersectionVisuals();
        RefreshSelectionPreview();
        UpdateControlInteractable();
    }

    private void ApplyCollapseVisuals(bool animate)
    {
        bool hasCollapse = activeCollapse.HasValue;
        Vector2 posA = Vector2.zero, n = Vector2.zero;
        float len = 0f;

        if (hasCollapse)
        {
            var (a, b) = activeCollapse.Value;
            posA = IntersectionPosition(a.rowLine, a.colLine);
            Vector2 posB = IntersectionPosition(b.rowLine, b.colLine);
            Vector2 delta = posB - posA;
            len = delta.magnitude;
            n = len > 0.001f ? delta / len : Vector2.zero;
        }

        foreach (var kvp in cellViews)
        {
            var (r, c) = kvp.Key;
            var view = kvp.Value;
            bool isHidden = hidden[r, c];
            view.SetHidden(isHidden, animate);

            Vector2 origin = CellOrigin(r, c);
            Vector2 target = origin;

            if (hasCollapse && !isHidden)
            {
                Vector2 center = CellCenter(r, c);
                float t = Vector2.Dot(center - posA, n);
                float shift = Mathf.Clamp(t, 0f, len);
                target = origin - n * shift;
            }

            view.MoveTo(target, animate);
        }
    }

    private void OnReset()
    {
        if (running) return;

        accumulator = 0;
        won = false;
        foreach (var row in grid)
            foreach (var cell in row)
                if (cell.kind == HackOpKind.CmpGoal) cell.completed = false;

        RenderAll();
    }

    private void OnStep()
    {
        if (running || locked || won) return;
        StartCoroutine(StepSequence());
    }

    private List<(int row, int col)> BuildReadingOrder()
    {
        var order = new List<(int, int)>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (!hidden[r, c]) order.Add((r, c));
        return order;
    }

    private IEnumerator StepSequence()
    {
        running = true;
        UpdateControlInteractable();

        var order = BuildReadingOrder();
        int i = 0;

        while (i < order.Count)
        {
            var (row, col) = order[i];
            var cellData = grid[row][col];
            cellViews.TryGetValue((row, col), out var view);
            view?.SetHighlighted(true);

            switch (cellData.kind)
            {
                case HackOpKind.Adc:
                    accumulator = (accumulator + cellData.amount) % 256;
                    view?.ShowPopup($"+{cellData.amount}", view.textNormal);
                    break;

                case HackOpKind.Jmp:
                {
                    view?.ShowPopup("JMP", view.textNeutral);
                    int targetIdx = order.FindIndex(p => p.row == cellData.targetRow && p.col == cellData.targetCol);
                    if (targetIdx != -1) i = targetIdx - 1;
                    break;
                }

                case HackOpKind.Set:
                {
                    view?.ShowPopup("SET", view.textNeutral);
                    if (cellData.targetRow >= 0 && cellData.targetRow < rows && cellData.targetCol >= 0 && cellData.targetCol < cols)
                    {
                        OpcodeTable.Apply(grid[cellData.targetRow][cellData.targetCol], accumulator);
                        if (cellViews.TryGetValue((cellData.targetRow, cellData.targetCol), out var targetView))
                            targetView.Render(grid[cellData.targetRow][cellData.targetCol]);
                    }
                    break;
                }

                case HackOpKind.CmpGoal:
                    if (accumulator >= cellData.threshold)
                    {
                        cellData.completed = true;
                        view?.Render(cellData);
                        view?.ShowPopup("PASS", view.textSuccess);
                    }
                    else
                    {
                        view?.ShowPopup(accumulator.ToString(), view.textNeutral);
                    }
                    break;

                case HackOpKind.CmpSecurity:
                    if (accumulator >= cellData.threshold)
                    {
                        view?.ShowPopup("TRIPPED", view.textDanger);
                        attemptsRemaining--;
                        UpdateStatusText();
                        yield return new WaitForSeconds(stepDelay);
                        view?.SetHighlighted(false);
                        if (attemptsRemaining <= 0) locked = true;
                        running = false;
                        RenderAll();
                        yield break;
                    }
                    else
                    {
                        view?.ShowPopup(accumulator.ToString(), view.textNeutral);
                    }
                    break;

                default:
                    view?.ShowPopup("--", view.textNeutral);
                    break;
            }

            UpdateStatusText();
            yield return new WaitForSeconds(stepDelay);
            view?.SetHighlighted(false);
            i++;
        }

        bool allGoalsMet = grid.SelectMany(row => row).Where(c => c.kind == HackOpKind.CmpGoal).All(c => c.completed);
        if (allGoalsMet)
        {
            won = true;
            OnHackSuccess();
        }

        running = false;
        RenderAll();
    }

    private void OnHackSuccess()
    {
        session.connectedFSUnlocked = true;
        StartCoroutine(ReturnToDefaultAfterDelay(1.2f));
    }

    private IEnumerator ReturnToDefaultAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        RequestExitToDefault();
    }

    private void UpdateStatusText()
    {
        accumulatorText.text = $"{accumulator}";
        accumulatorSubText.text = $"(0x{accumulator:X2})";
        attemptsText.text = locked ? "TERMINAL LOCKED" : $"{attemptsRemaining} attempts remaining";
    }

    private void UpdateControlInteractable()
    {
        collapseButton.interactable = !running && !locked && !won && !activeCollapse.HasValue && selectedPoints.Count == 2;
        expandButton.interactable = !running && !locked && !won && activeCollapse.HasValue;
        stepButton.interactable = !running && !locked && !won;
        resetButton.interactable = !running;
    }

    private void RenderAll()
    {
        foreach (var kvp in cellViews) kvp.Value.Render(grid[kvp.Key.Item1][kvp.Key.Item2]);

        UpdateStatusText();
        UpdateControlInteractable();

        if (locked) messageText.text = "-3- TERMINAL LOCKED - Out of attempts!";
        else if (won) messageText.text = ">;3 TERMINAL HACK SUCCESSFUL - File Access Granted!";
        else messageText.text = "Meet the requirements for each of the <color=#69D8FA><b>CoMPare blocks<color=#FEB65A></b> to complete the terminal hack! Just don't trigger the <color=#F3341B><b>SECurity blocks<color=#FEB65A></b> or you'll get locked out!";
    }
}

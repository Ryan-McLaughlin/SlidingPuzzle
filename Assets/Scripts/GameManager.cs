using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager: MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;

    private List<Transform> pieces;
    private int emptyLocation;
    private bool shuffling = false;

    public int size;
    public int moves;

    void Start()
    {
        pieces = new List<Transform>();
        CreateGamePieces(0.01f, size);
        Shuffle(size, moves);
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        //Debug.Log("GameManager.OnClick()");
        if(context.started)
        {
            // Debug.Log("context.started");
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePosition), Vector2.zero);
            if(hit)
            {
                Debug.Log($"pieces.Count: {pieces.Count}");
                for(int i = 0; i < pieces.Count; i++)
                {
                    // Use a slightly different hit check to see if the piece was clicked.
                    if(hit.transform == pieces[i])
                    {
                        if(SwapIfValid(i, -size, size)) { break; }
                        if(SwapIfValid(i, +size, size)) { break; }
                        if(SwapIfValid(i, -1, 0)) { break; }
                        if(SwapIfValid(i, +1, size - 1)) { break; }
                    }
                }
            }
        }
    }

    private void CreateGamePieces(float gapThickness, int gridSize)
    {
        float width = 1 / (float)gridSize;

        for(int row = 0; row < gridSize; row++)
        {
            for(int col = 0; col < gridSize; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);

                pieces.Add(piece);

                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                                                  +1 - (2 * width * row) + width,
                                                  0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * gridSize) + col}";

                if((row == gridSize - 1) && (col == gridSize - 1))
                {
                    emptyLocation = (gridSize * gridSize) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                    Vector2[] uv = new Vector2[4];

                    uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));

                    mesh.uv = uv;
                }
            }
        }
    }

    private bool CheckCompletion()
    {
        for(int i = 0; i < pieces.Count; i++)
        {
            if(pieces[i].name != $"{i}")
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle(size, moves);
        shuffling = false;
    }

    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        Debug.Log("GameManager.SwapIfValid");
        if(((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            (pieces[i].localPosition, pieces[i + offset].localPosition) = ((pieces[i + offset].localPosition, pieces[i].localPosition));
            emptyLocation = i;

            // Check for completion after a piece has been moved
            if(!shuffling && CheckCompletion())
            {
                shuffling = true;
                StartCoroutine(WaitShuffle(0.5f));
            }

            return true;
        }

        return false;
    }

    private void Shuffle(int gridSize, int moves)
    {
        //int swaps = size * size; // Use a number of swaps relative to the puzzle size.
        int lastSwapIndex = -1; // Keep track of the last moved piece to prevent immediately undoing a move.

        for(int i = 0; i < moves; i++)
        {
            // Find all valid neighbors of the empty space.
            List<int> validMoves = new List<int>();

            // Check above
            if(emptyLocation - gridSize >= 0) validMoves.Add(emptyLocation - gridSize);
            // Check below
            if(emptyLocation + gridSize < pieces.Count) validMoves.Add(emptyLocation + gridSize);
            // Check left
            if(emptyLocation % gridSize != 0) validMoves.Add(emptyLocation - 1);
            // Check right
            if(emptyLocation % gridSize != gridSize - 1) validMoves.Add(emptyLocation + 1);

            // Remove the last swapped piece from the list of valid moves to avoid undoing the previous move.
            if(lastSwapIndex != -1)
            {
                validMoves.Remove(lastSwapIndex);
            }

            // Pick a random neighbor to swap with the empty space.
            int randomIndex = Random.Range(0, validMoves.Count);
            int pieceToSwapIndex = validMoves[randomIndex];

            // Perform the swap.
            (pieces[emptyLocation].localPosition, pieces[pieceToSwapIndex].localPosition) = (pieces[pieceToSwapIndex].localPosition, pieces[emptyLocation].localPosition);
            (pieces[emptyLocation], pieces[pieceToSwapIndex]) = (pieces[pieceToSwapIndex], pieces[emptyLocation]);

            // Update the lastSwapIndex and emptyLocation.
            lastSwapIndex = emptyLocation;
            emptyLocation = pieceToSwapIndex;
        }
    }
}

/*/ Brute force shuffling
private void BruteShuffle()
{
    int count = 0;
    int last = 0;

    while(count < (size * size * size))
    {
        int rnd = Random.Range(0, size * size);

        if(rnd == last) { continue; }
        last = emptyLocation;

        if(SwapIfValid(rnd, -size, size))
        {
            count++;
        }
        else if(SwapIfValid(rnd, +size, size))
        {
            count++;
        }
        else if(SwapIfValid(rnd, -1, 0))
        {
            count++;
        }
        else if(SwapIfValid(rnd, +1, size - 1))
        {
            count++;
        }
    }
}
*/


using UnityEngine;

public class StateMachineBase<T> : MonoBehaviour where T : StateMachineBase<T>
{
	private StateBase<T> m_currentState;
	private StateBase<T> m_nextState;
	[SerializeField] private string currentStateName;

	public bool ChangeState(StateBase<T> _nextState)
	{
		bool bRet = m_nextState == null;
		m_nextState = _nextState;
		currentStateName = m_nextState.ToString();
		return bRet;
	}

	private void Update()
	{
		if (m_nextState != null)
		{
			if (m_currentState != null)
			{
				m_currentState.OnExitState();
			}
			m_currentState = m_nextState;
			m_currentState.OnEnterState();
			if(m_currentState == m_nextState)
            {
				m_nextState = null;
			}
		}

		if (m_currentState != null)
		{
			m_currentState.OnUpdate();
		}
	}
}
import apiClient from './client'
import type { AgentChatRequest, AgentChatResponse } from './types'

export async function sendAgentMessage(
  request: AgentChatRequest,
): Promise<AgentChatResponse> {
  const { data } = await apiClient.post<AgentChatResponse>(
    '/agent/chat',
    request,
  )
  return data
}

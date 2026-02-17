import { useInfiniteQuery } from '@tanstack/react-query'
import { getTransactions } from '@/lib/api/transactions'

const PAGE_SIZE = 20

export function useInfiniteTransactions(accountId: string) {
  return useInfiniteQuery({
    queryKey: ['transactions', accountId],
    queryFn: ({ pageParam }) =>
      getTransactions(accountId, pageParam ?? undefined, PAGE_SIZE),
    initialPageParam: null as string | null,
    getNextPageParam: (lastPage) =>
      lastPage.pagination.hasMore ? lastPage.pagination.nextCursor : undefined,
    enabled: !!accountId,
  })
}

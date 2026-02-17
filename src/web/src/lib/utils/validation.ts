import { z } from 'zod'

export const transferSchema = z.object({
  fromAccountId: z.string().min(1, 'Please select source account'),
  toAccountId: z.string().min(1, 'Please select destination account'),
  amount: z
    .number({ error: 'Amount must be a number' })
    .positive('Amount must be greater than zero')
    .max(1_000_000, 'Amount exceeds maximum limit'),
  description: z
    .string()
    .max(200, 'Description must be 200 characters or fewer')
    .optional(),
})

export type TransferFormData = z.infer<typeof transferSchema>

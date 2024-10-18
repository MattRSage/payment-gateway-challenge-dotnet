Assumptions
1. The requirements do not specify if we want to store `Rejected` payments. I have assumed that we will store them as we have a Rejected status on the `PaymentStatus` enum. However, I have assumed we should not return a `Rejected` payment, as the requirements state we should only return `Authorized` or `Declined`.
2. I have assumed for `Rejected` payments, we will return a 400 bad request along with the validation errors.
3. `authorization_code` isn't part of the response, but I assume we need it later for other processing, so I am storing it in the repository for the payment.

Improvements
1. For errors, I would use a standard error response like the ProblemDetails RFC 9457 https://www.rfc-editor.org/rfc/rfc9457
2. If the application has promise to grow, I would add layers to the service and choose an appropriate architecture like CA or HA
3. We should make the post payment endpoint idempotent to avoid creating multiple payments for the same request. This could be achieved by sending an idempotency key as a request header.
4. It would be a good idea to pass cancellation tokens from the controller down to the HttpClient request. If we were using a real DB, we could pass the cancellation token there too.
5. It makes sense to include a CreatedAt timestamp on the Payment.
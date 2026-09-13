// Text-only Gemini adapter. No SDK, tools, files, search, or provider-side chat IDs.
const categories = [
  'HARM_CATEGORY_HARASSMENT',
  'HARM_CATEGORY_HATE_SPEECH',
  'HARM_CATEGORY_SEXUALLY_EXPLICIT',
  'HARM_CATEGORY_DANGEROUS_CONTENT'
];

export function buildGeminiRequest(messages, instructions) {
  return {
    systemInstruction: { parts: [{ text: `${instructions} Use plain text, at most 100 words and 900 characters.` }] },
    contents: messages.map(message => ({
      role: message.role === 'assistant' ? 'model' : 'user',
      parts: [{ text: message.content }]
    })),
    safetySettings: categories.map(category => ({ category, threshold: 'BLOCK_LOW_AND_ABOVE' })),
    generationConfig: { candidateCount: 1, maxOutputTokens: 2048, responseMimeType: 'text/plain' }
  };
}

const acceptableRatings = ratings => ratings === undefined || (Array.isArray(ratings) && ratings.every(rating =>
  rating && (rating.blocked === undefined || rating.blocked === false) && rating.probability === 'NEGLIGIBLE'));

// Fail closed on blocked, partial, malformed, or non-text output. Never display thoughts.
// These checks supplement Google's filters; they are not a clinical safety assessment.
export function readGeminiReply(response) {
  if (!response || typeof response !== 'object' || response.error) return null;
  const feedback = response.promptFeedback;
  if (feedback !== undefined && (!feedback || typeof feedback !== 'object' || Array.isArray(feedback)
    || (feedback.blockReason && feedback.blockReason !== 'BLOCK_REASON_UNSPECIFIED')
    || !acceptableRatings(feedback.safetyRatings))) return null;
  if (!Array.isArray(response.candidates) || response.candidates.length !== 1) return null;
  const candidate = response.candidates[0];
  if (!candidate || candidate.finishReason !== 'STOP' || !acceptableRatings(candidate.safetyRatings)
    || candidate.content?.role !== 'model' || !Array.isArray(candidate.content.parts)) return null;
  const parts = candidate.content.parts;
  if (!parts.length || parts.some(part => !part || typeof part.text !== 'string'
    || (part.thought !== undefined && typeof part.thought !== 'boolean')
    || Object.keys(part).some(key => !['text', 'thought', 'thoughtSignature'].includes(key)))) return null;
  const text = parts.filter(part => part.thought !== true).map(part => part.text).join('\n').trim();
  // Do not truncate a safety message halfway through a sentence.
  return text.length > 0 && text.length <= 900 ? text : null;
}
